using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NuGet.Common;
using NuGet.Configuration;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using static SimurgWeb.Pages.ProjectDetail;

namespace SimurgWeb.Services
{
    public class ProjectService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SimurgContext _dbContext;
        private static SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1); // Aynı anda 1 işlem

        public ProjectService(SimurgContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<TblProject>> GetProjectsAsync(string token, bool? isActive = null, int page = 1, int pageSize = 10, string searchTerm = "")
        {
            await _searchSemaphore.WaitAsync(); // Kilidi al

            try
            {
                var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
                if (getUser == null)
                {
                    throw new Exception("Kullanıcı bulunamadı");
                }

                var query = _dbContext.TblProjects
                    .Where(p => !p.IsDeleted.Value && _dbContext.TblProjectAuthorizes
                        .Any(a => a.UserId == getUser.Id && a.ProjectId == p.Id));

                if (!query.Any())
                {
                    return new List<TblProject>();
                }

                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p => p.ProjectName.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(p => p.CreatedTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            finally
            {
                _searchSemaphore.Release(); // Kilidi serbest bırak
            }
        }
        public int GetProjectsCount(string token, bool? isActive = null)
        {
            var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
            var query = _dbContext.TblProjects
                .Where(p => !p.IsDeleted.Value && _dbContext.TblProjectAuthorizes
                    .Any(a => a.UserId == getUser.Id && a.ProjectId == p.Id));

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            return query.Count();
        }
        public async Task<ProjectDetailModel> GetProjectByIdAsync(string token, int projectId)
        {
            if (projectId == 0)
            {
                return new ProjectDetailModel();
            }

            var getUser = await _dbContext.TblUsers
                .Where(p => p.Username == GetUserName(token))
                .Select(p => new { p.Id }) // Sadece gerekli alanı çek
                .FirstOrDefaultAsync();

            if (getUser == null)
            {
                return new ProjectDetailModel(); // Kullanıcı bulunamazsa boş model dön
            }

            var res = await _dbContext.TblProjects
                .Include(p => p.CreatedUser)
                .Where(p => p.Id == projectId && _dbContext.TblProjectAuthorizes
                    .Any(a => a.UserId == getUser.Id && a.ProjectId == p.Id))
                .Select(p => new ProjectDetailModel
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    CreatedBy = p.CreatedUser.Username,
                    CreatedDate = p.CreatedTime,
                    IsActive = p.IsActive,
                    Explanation = p.Explanation
                })
                .FirstOrDefaultAsync();

            return res ?? new ProjectDetailModel(); // Eğer veri bulunamazsa boş model döner

        }
        public async Task<ProjectDetailModel> UpdateProjectAsync(string token, ProjectDetailModel project)
        {
            if (project.Id == 0)
            {
                var getUserId = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
                var addItem = new TblProject();
                addItem.ProjectName = project.ProjectName;
                addItem.IsActive = true;                
                addItem.CreatedUserId = getUserId.Id;
                _dbContext.TblProjects.Add(addItem);
                _dbContext.SaveChanges();

                var addItemAuthorize = new TblProjectAuthorize();
                addItemAuthorize.ProjectId = addItem.Id;
                addItemAuthorize.UserId = getUserId.Id;
                _dbContext.TblProjectAuthorizes.Add(addItemAuthorize);
                _dbContext.SaveChanges();

                project.CreatedDate = addItem.CreatedTime;
                project.Id = addItem.Id;
            }
            else
            {
                var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
                //var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();

                var update = _dbContext.TblProjects.FirstOrDefault(p => p.Id == project.Id && _dbContext.TblProjectAuthorizes.Any(a => a.UserId == getUser.Id && a.ProjectId == p.Id));
                if (update == null)
                {
                    throw new Exception("Ekleme işlemi başarısız");
                }
                update.ProjectName = project.ProjectName;
                update.Explanation = project.Explanation;
                _dbContext.TblProjects.Update(update);
                _dbContext.SaveChanges();                
            }
            return project;
        }
        public async Task<List<Expense>> GetExpenseList(string token, int projectId)
        {
            var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
            var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();
            if (authorizedProjectIds.Count < 1)
            {
                return new List<Expense>();
            }
            if (!authorizedProjectIds.Contains(projectId))
            {
                throw new Exception("Yetkisiz işlem");
            }

            return  await _dbContext.TblItems.Where(p => p.ProjectId == projectId && p.IsActive && p.IsExpenses == true)
                .Select(p => new Expense
                { 
                    Amount = p.Price,
                    Description = p.Definition,
                    Id = p.Id,
                    IsFile = (p.InvoiceFile != null && p.InvoiceFile.Length > 0),
                    AddedFile = p.InvoiceFile
                }).ToListAsync();
        }
        public async Task<List<Income>> GetIncomeList(string token,int projectId)
        {
            var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
            var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();
            if (authorizedProjectIds.Count < 1)
            {
                return new List<Income>();
            }
            if (!authorizedProjectIds.Contains(projectId))
            {
                throw new Exception("Yetkisiz işlem");
            }

            return await _dbContext.TblItems.Where(p => p.ProjectId == projectId && p.IsActive && p.IsExpenses == false)
                .Select(p => new Income
                {
                    Amount = p.Price,
                    Description = p.Definition,
                    Id = p.Id,
                    IsFile = (p.InvoiceFile != null && p.InvoiceFile.Length > 0),
                    AddedFile = p.InvoiceFile
                }).ToListAsync();
        }
        public async Task<Expense> AddExpense(string token, int projectId,Expense item)
        {
            try
            {
                var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
                var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();
                if (!authorizedProjectIds.Contains(projectId))
                {
                    throw new Exception("Yetkisiz işlem");
                }

                byte[]? fileBytes = null;
                if (item.File != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        // Dosyayı stream üzerinden okuyoruz ve belleğe yazıyoruz
                        await item.File.OpenReadStream().CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }                

                var getUserId = await _dbContext.TblUsers.FirstOrDefaultAsync(p => p.Username == GetUserName(token));
                var addItem = new TblItem();
                addItem.ProjectId = projectId;
                addItem.CreatedUserId = getUserId.Id;
                addItem.Definition = item.Description;
                addItem.IsExpenses = true;
                addItem.IsActive = true;
                addItem.Price = item.Amount;
                addItem.InvoiceFile = item.File != null ? fileBytes : null;
                _dbContext.TblItems.Add(addItem);
                _dbContext.SaveChanges();
                item.Id = addItem.Id;

                var addLog = new TblLog();
                addLog.RecordId = addItem.Id;
                addLog.CreatedUserId = getUserId.Id;
                addLog.Definition = $"{projectId} id'li projeye yeni gider eklendi. Eklenen fiyat {item.Amount} - açıklama {item.Description}";
                addLog.CreatedTime = DateTime.Now;
                addLog.PageName = "Expense";
                addLog.Action = "Add";
                _dbContext.TblLogs.Add(addLog);
                _dbContext.SaveChanges();

                return item;
            }
            catch (Exception ex)
            {
                return new Expense {Id = -1 };
            }
            
        }
        public async Task<Income> AddIncome(string token, int projectId, Income item)
        {
            try
            {
                var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
                var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();
                if (!authorizedProjectIds.Contains(projectId))
                {
                    throw new Exception("Yetkisiz işlem");
                }

                byte[]? fileBytes = null;
                if (item.File != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        // Dosyayı stream üzerinden okuyoruz ve belleğe yazıyoruz
                        await item.File.OpenReadStream().CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                var getUserId = await _dbContext.TblUsers.FirstOrDefaultAsync(p => p.Username == GetUserName(token));
                var addItem = new TblItem();
                addItem.ProjectId = projectId;
                addItem.CreatedUserId = getUserId.Id;
                addItem.Definition = item.Description;
                addItem.IsExpenses = false;
                addItem.IsActive = true;
                addItem.Price = item.Amount;
                addItem.InvoiceFile = item.File != null ? fileBytes : null;
                _dbContext.TblItems.Add(addItem);
                _dbContext.SaveChanges();
                item.Id = addItem.Id;

                var addLog = new TblLog();
                addLog.RecordId = addItem.Id;
                addLog.CreatedUserId = getUserId.Id;
                addLog.Definition = $"{projectId} id'li projeye yeni gelir eklendi. Eklenen fiyat {item.Amount} - açıklama {item.Description}";
                addLog.CreatedTime = DateTime.Now;
                addLog.PageName = "Income";
                addLog.Action = "Add";
                _dbContext.TblLogs.Add(addLog);
                _dbContext.SaveChanges();

                return item;
            }
            catch (Exception ex)
            {
                return new Income { Id = -1 };
            }

        }
        public async Task<bool> DeleteExpenseOrIncome(string token, int itemid)
        {
            var user = await _dbContext.TblUsers.FirstOrDefaultAsync(p => p.Username == GetUserName(token));
            var item = await _dbContext.TblItems.FirstOrDefaultAsync(p => p.Id == itemid);

            var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == user.Id).Select(p => p.ProjectId).ToList();
            if (!authorizedProjectIds.Contains(item.ProjectId))
            {
                throw new Exception("Yetkisiz işlem");
            }

            item.IsActive = false;
            item.DeleteUserId = user.Id;
            _dbContext.TblItems.Update(item);
            _dbContext.SaveChanges();

            var logtext = item.IsExpenses ? "Gider" : "Gelir";

            var addLog = new TblLog();
            addLog.RecordId = item.Id;
            addLog.CreatedUserId = user.Id;
            addLog.Definition = $"{item.ProjectId} id'li projede {logtext} silindi. Silinen fiyat {item.Price} - açıklama {item.Definition}";
            addLog.CreatedTime = DateTime.Now;
            addLog.PageName = item.IsExpenses ? "Expense" : "Income";
            addLog.Action = "Delete";
            _dbContext.TblLogs.Add(addLog);
            _dbContext.SaveChanges();

            return true;
        }
        public async Task<bool> ProjectCloseAsync(string token, int projectId)
        {
            var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
            var authorizedProjectIds = _dbContext.TblProjectAuthorizes.Where(p => p.UserId == getUser.Id).Select(p => p.ProjectId).ToList();
            if (!authorizedProjectIds.Contains(projectId))
            {
                throw new Exception("Yetkisiz işlem");
            }

            var user = await _dbContext.TblUsers.FirstOrDefaultAsync(p => p.Username == GetUserName(token));
            var project = _dbContext.TblProjects.FirstOrDefault(p=>p.Id == projectId);
            project.IsActive = false;
            _dbContext.TblProjects.Update(project);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<List<ProjectAuthorizeUser>> GetUserList(string token, int projectId)
        {
            var result = new List<ProjectAuthorizeUser>();
            var users = await _dbContext.TblUsers.Where(p=>p.IsActive).ToListAsync();
            var userAuthorize = await _dbContext.TblProjectAuthorizes.ToListAsync();
            foreach (var item in users)
            {
                var addItem = new ProjectAuthorizeUser()
                {
                    UserName = item.Username,
                    ProjectId = projectId,
                    IsSelected = userAuthorize.Any(P=>P.ProjectId == projectId && P.UserId == item.Id),
                    UserId = item.Id
                };
                result.Add(addItem);
            }
            return result;
        }
        public async Task<bool> UserAuthorizeAddOrDelete(string token, List<ProjectAuthorizeUser> authroizeList)
        {
            try
            {
                _dbContext.TblProjectAuthorizes.RemoveRange();
                foreach (var item in authroizeList)
                {
                    if (item.IsSelected)
                    {
                        var isAny = await _dbContext.TblProjectAuthorizes.FirstOrDefaultAsync(p => p.ProjectId == item.ProjectId && p.UserId == item.UserId);
                        if (isAny == null)
                        {
                            var addItem = new TblProjectAuthorize();
                            addItem.ProjectId = item.ProjectId;
                            addItem.UserId = item.UserId;
                            await _dbContext.TblProjectAuthorizes.AddAsync(addItem);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var isAny = await _dbContext.TblProjectAuthorizes.FirstOrDefaultAsync(p => p.ProjectId == item.ProjectId && p.UserId == item.UserId);
                        _dbContext.TblProjectAuthorizes.Remove(isAny);
                        await _dbContext.SaveChangesAsync();
                    }
                }                

                return true;
            }
            catch (Exception ex)
            {
                return true;
            }            
        }
        private string GetUserName(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault()?.Value;
        }
        public async Task<bool> ProjectDeleteAsync(string token, int itemid)
        {
            var getUser = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));
            if (getUser == null)
            {
                throw new Exception("Kullanıcı bulunamazsa");
            }

            var query = _dbContext.TblProjects
                .Any(p => !p.IsDeleted.Value && _dbContext.TblProjectAuthorizes
                    .Any(a => a.UserId == getUser.Id && a.ProjectId == p.Id));
            if (query)
            {
                var deleteItem = _dbContext.TblProjects.FirstOrDefault(p => p.Id == itemid);
                deleteItem.IsDeleted = true;
                _dbContext.TblProjects.Update(deleteItem);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            throw new Exception("Silme işlemi başarısız oldu!!");
        }
    }


    public class ProjectAuthorizeUser
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}

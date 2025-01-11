using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;

namespace SimurgWeb.Services
{    
    public class LogService
    {
        private readonly SimurgContext _dbContext;
        private int totalPageCount = 0;
        public LogService(SimurgContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LogItem>> GetLogs(int pageNumber, int pageSize = 100)
        {
            var returnList = new List<LogItem>();
            var tblUser = await _dbContext.TblUsers.ToListAsync();
            var logs = await _dbContext.TblLogs.OrderByDescending(p=>p.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            foreach (var item in logs)
            {
                var addItem = new LogItem();
                addItem.Action = item.PageName;
                addItem.ProjectName = GetProjectName(item.RecordId);
                addItem.Message = item.Definition;
                addItem.CreatedTime = item.CreatedTime;
                addItem.UserName = tblUser.FirstOrDefault(p => p.Id == item.CreatedUserId).Username;
                addItem.IsDeleted = item.Action == "Delete" ? true : false;
                returnList.Add(addItem);
            }

            var totalRecords = _dbContext.TblLogs.Count();
            totalPageCount = (int)Math.Ceiling((double)totalRecords / pageSize);

            return returnList;
        }

        public int GetTotalPageCount()
        {
            return totalPageCount;
        }

        private string GetProjectName(int recordId)
        {
            var item = _dbContext.TblItems.Include(p => p.Project).FirstOrDefault(p => p.Id == recordId);
            return item.Project.ProjectName;
        }
    }
    public class LogItem
    {
        public string ProjectName { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Action { get; set; }
        public bool IsDeleted { get; set; }
        public string Message { get; set; }
    }
}

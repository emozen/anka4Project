using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace SimurgWeb.Services
{    
    public class PeriodService
    {
        private readonly string _fileStoragePath;
        private readonly SimurgContext _dbContext;

        public PeriodService(SimurgContext dbContext)
        {
            _dbContext = dbContext;

            // Dosyaların saklanacağı ana klasörü belirleyin
            _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles/Period");

            // Klasör yoksa oluştur
            if (!Directory.Exists(_fileStoragePath))
            {
                Directory.CreateDirectory(_fileStoragePath);
            }
        }

        public async Task<List<PeriodMaster>> GetPeriodListAsync()
        {
            var response = await _dbContext.TblPeriods
            .Include(p => p.TblPeriodItems)
            .OrderByDescending(p=>p.Year).ThenByDescending(p=>p.Month)
            .Select(p=> new PeriodMaster { 
                ExpenceTotalPrice = p.TblPeriodItems.Where(x=> !x.IsIncoming.Value).Sum(x=>x.Price).Value,
                IncomingTotalPrice = p.TblPeriodItems.Where(x => x.IsIncoming.Value).Sum(x => x.Price).Value,
                Id = p.Id,
                Name = p.Name,
                PeriodItems = p.TblPeriodItems.Select(x=>new PeriodItem { 
                    Id = x.Id,
                    Description = x.Description,
                    IsIncoming = x.IsIncoming.Value ? "Gelir" : "Gider",
                    PeriodId = x.Id,
                    Price = x.Price
                }).ToList()
            }).ToListAsync();

            var nowYear = DateTime.Now.Year;
            var nowMonth = DateTime.Now.Month;

            if (response.Count(p=>p.Name == GetPeriodName(nowYear, nowMonth)) < 1)
            {
                var newItem = new TblPeriod();
                newItem.Year = nowYear;
                newItem.Month = nowMonth;
                newItem.Name = GetPeriodName(nowYear, nowMonth);
                await _dbContext.TblPeriods.AddAsync(newItem);
                await _dbContext.SaveChangesAsync();

                response.Add(new PeriodMaster { 
                    Id = newItem.Id,
                    Name = newItem.Name
                });
            }

            return response;
        }

        public async Task<bool> AddPeriodItem(PeriodItem item)
        {
            try
            {
                if (item.Id == 0)
                {
                    var addItem = new TblPeriodItem();
                    addItem.PeriodId = item.PeriodId;
                    addItem.IsIncoming = item.IsIncoming == "Gelir";
                    addItem.Price = item.Price;
                    addItem.Description = item.Description;
                    await _dbContext.TblPeriodItems.AddAsync(addItem);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    var updateItem = _dbContext.TblPeriodItems.FirstOrDefault(p => p.Id == item.Id);
                    if (updateItem != null)
                    {
                        updateItem.PeriodId = item.PeriodId;
                        updateItem.IsIncoming = item.IsIncoming == "Gelir";
                        updateItem.Price = item.Price;
                        updateItem.Description = item.Description;
                        _dbContext.TblPeriodItems.Update(updateItem);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> DeletePeriodItem(PeriodItem item)
        {
            try
            {
                var deleteItem = _dbContext.TblPeriodItems.FirstOrDefault(p => p.Id == item.Id);
                if (deleteItem != null)
                {
                    _dbContext.TblPeriodItems.Remove(deleteItem);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetPeriodName(int? year, int? month)
        {
            switch (month)
            {
                case 1:
                    return $"{year} Ocak";
                case 2:
                    return $"{year} Şubat";
                case 3:
                    return $"{year} Mart";
                case 4:
                    return $"{year} Nisan";
                case 5:
                    return $"{year} Mayıs";
                case 6:
                    return $"{year} Haziran";
                case 7:
                    return $"{year} Temmuz";
                case 8:
                    return $"{year} Auğustos";
                case 9:
                    return $"{year} Eylül";
                case 10:
                    return $"{year} Ekim";
                case 11:
                    return $"{year} Kasım";
                case 12:
                    return $"{year} Aralık";
                default:
                    return $"Null";
            }
        }

        /// <summary>
        /// Proje klasörüne Excel dosyası yükler.
        /// </summary>
        /// <param name="stream">Dosya içeriği</param>
        /// <param name="fileName">Dosya adı</param>
        /// <param name="projectName">Proje adı</param>
        /// <returns></returns>
        public async Task UploadExcelFileToProject(Stream stream, string fileName, string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentException("Proje adı boş olamaz.", nameof(projectName));
            }

            // Proje ismine göre klasör oluştur
            string projectFolderPath = Path.Combine(_fileStoragePath, projectName);

            if (!Directory.Exists(projectFolderPath))
            {
                Directory.CreateDirectory(projectFolderPath);
            }

            // Mevcut dosyaları sil (her proje için tek dosya mantığı)
            var files = Directory.GetFiles(projectFolderPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            // Dosyayı ilgili klasöre kaydet
            string filePath = Path.Combine(projectFolderPath, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
        }

        /// <summary>
        /// Proje klasöründen Excel dosyasını okur.
        /// </summary>
        /// <param name="projectName">Proje adı</param>
        /// <returns>Dosya içeriği</returns>
        public async Task<byte[]> GetExcelFileByProject(string projectName)
        {
            string projectFolderPath = Path.Combine(_fileStoragePath, projectName);

            // Klasörde herhangi bir dosya olup olmadığını kontrol et
            if (Directory.Exists(projectFolderPath))
            {
                var files = Directory.GetFiles(projectFolderPath);
                if (files.Length > 0)
                {
                    string filePath = files.First(); // İlk dosyayı al
                    return await File.ReadAllBytesAsync(filePath);
                }
            }

            throw new FileNotFoundException($"'{projectName}' için yüklenmiş dosya bulunamadı.");
        }

        /// <summary>
        /// Proje klasöründen Excel dosyasını siler.
        /// </summary>
        /// <param name="projectName">Proje adı</param>
        public async Task DeleteExcelFileByProject(string projectName)
        {
            string projectFolderPath = Path.Combine(_fileStoragePath, projectName);

            // Klasördeki tüm dosyaları sil
            if (Directory.Exists(projectFolderPath))
            {
                var files = Directory.GetFiles(projectFolderPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

            await Task.CompletedTask;
        }
    }

    public class PeriodMaster
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal IncomingTotalPrice { get; set; }
        public decimal ExpenceTotalPrice { get; set; }
        public List<PeriodItem>? PeriodItems { get; set; }
    }
    public class PeriodItem
    {
        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string? IsIncoming { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
    }
}

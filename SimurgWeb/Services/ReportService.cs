using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;

namespace SimurgWeb.Services
{
    public class ReportService
    {
        private readonly string _fileStoragePath;
        private readonly SimurgContext _dbContext;

        public ReportService(SimurgContext dbContext)
        {
            _dbContext = dbContext;

            // Dosyaların saklanacağı ana klasörü belirleyin
            _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            // Klasör yoksa oluştur
            if (!Directory.Exists(_fileStoragePath))
            {
                Directory.CreateDirectory(_fileStoragePath);
            }
        }

        public async Task<List<ProjectList>> GetProjectList()
        {
            return await _dbContext.TblProjects.Select(p => new ProjectList { Id = p.Id, ProjectName = p.ProjectName }).ToListAsync();
        }

        public async Task<List<IncomeExpenseItems>> GetIncomeExpenseList(bool isExpense, int projectId)
        {
            return await _dbContext.TblItems
                .Where(p => p.ProjectId == projectId && p.IsActive && p.IsExpenses == isExpense)
                .Select(p => new IncomeExpenseItems { Description = p.Definition, Amount = p.Price })
                .ToListAsync();
        }

        public decimal GetTotalAmount(List<IncomeExpenseItems> list)
        {
            return list.Sum(p => p.Amount);
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

    public class ProjectList
    {
        public int Id { get; set; }
        public string? ProjectName { get; set; }
    }

    public class IncomeExpenseItems
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}

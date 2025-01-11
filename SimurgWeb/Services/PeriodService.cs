using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace SimurgWeb.Services
{
    public class PeriodService
    {
        private readonly SimurgContext _dbContext;

        public PeriodService(SimurgContext dbContext)
        {
            _dbContext = dbContext;
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

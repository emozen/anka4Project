﻿using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;

namespace SimurgWeb.Services
{
    public class ReportService
    {        
        private readonly SimurgContext _dbContext;

        public ReportService(SimurgContext dbContext)
        {
            _dbContext = dbContext;            
        }

        public async Task<List<TblCustomer>> GetCustomersAsync()
        {
            return await _dbContext.TblCustomers.Where(p=>p.DeletedTime == null).ToListAsync();
        }

        public async Task<List<TblProject>> GetProjectsAsync(int customerId)
        {
            return await _dbContext.TblProjects.Where(p=>p.CustomerId == customerId && p.IsDeleted == false).ToListAsync();
        }

        public async Task<List<ProjectReport>> GetIncomeExpenseList(int projectId)
        {
            var res = _dbContext.TblItems.Where(p => p.ProjectId == projectId && p.IsActive);
                
            res.Include(p => p.Project).ThenInclude(p => p.Customer);

            return await res.Select(p => new ProjectReport { 
                Amount = p.Price,
                Customer = p.Project.Customer.CustomerName,
                Project = p.Project.ProjectName,
                IsIncome = !p.IsExpenses
            }).ToListAsync();
        }

        public async Task<decimal> GetTotalAmount(int projectId)
        {
            var res = _dbContext.TblItems.Where(p => p.ProjectId == projectId && p.IsActive);

            return await res.SumAsync(p=>p.Price);
        }

        public DoughnutReport GetDoughnutReport(int projectId, DateTime date)
        {
            try
            {
                var res = _dbContext.TblProjects
                    .Include(p => p.TblItems)
                    .Where(p => p.Id == projectId &&
                                p.TblItems.Any(x => x.IsActive) &&
                                p.StartDatetime.HasValue &&
                                p.StartDatetime.Value.Year == date.Year &&
                                p.StartDatetime.Value.Month == date.Month)
                    .Select(p => new
                    {
                        ExpenseTotal = p.TblItems.Where(x => x.IsExpenses && x.IsActive).Sum(x => x.Price), // Giderler
                        IncomeTotal = p.TblItems.Where(x => !x.IsExpenses && x.IsActive).Sum(x => x.Price)  // Gelirler
                    })
                    .ToList();

                var expenseTotal = res.Sum(x => x.ExpenseTotal);
                var incomeTotal = res.Sum(x => x.IncomeTotal);

                return new DoughnutReport
                {
                    Expense = expenseTotal,
                    Income = incomeTotal,
                    Total = expenseTotal + incomeTotal
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public class ProjectReport
    {
        public string? Customer { get; set; }
        public string Project { get; set; }
        public bool IsIncome { get; set; }
        public decimal Amount { get; set; }
    }

    public class DoughnutReport
    {
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Total { get; set; }
    }
}

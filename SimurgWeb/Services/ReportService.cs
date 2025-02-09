using Microsoft.EntityFrameworkCore;
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

        public async Task<List<ProjectList>> GetProjectList()
        {
            return await _dbContext.TblProjects.Select(p => new ProjectList { Id = p.Id, ProjectName = p.ProjectName }).ToListAsync();
        }

        public async Task<List<IncomeExpenseItems>> GetIncomeExpenseList(bool? isExpense, int projectId)
        {
            var res = _dbContext.TblItems.Where(p => p.ProjectId == projectId && p.IsActive);
            if (isExpense.HasValue)
            {
                res = res.Where(p => p.IsExpenses == isExpense);
            }

            return await res.Select(p => new IncomeExpenseItems { Description = p.Definition, Amount = p.Price }).ToListAsync();
        }

        public decimal GetTotalAmount(List<IncomeExpenseItems> list)
        {
            return list.Sum(p => p.Amount);
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

using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace SimurgWeb.Services
{
    public class CustomerService
    {
        private readonly SimurgContext _dbContext;

        public CustomerService(SimurgContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CustomerItem>> GetCustomerList(string token)
        {
            return await _dbContext.TblCustomers.Where(p=>p.DeletedTime == null).Select(p => new CustomerItem
            {
                Id = p.Id,
                CustomerName = p.CustomerName
            }).ToListAsync();
        }
        public async Task<bool> AddOrUpdateCustomer(string token, CustomerItem item)
        {
            if (item.Id == 0)
            {
                if (_dbContext.TblCustomers.Any(p => p.Id == item.Id))
                {
                    throw new Exception("Bu Müşteri eklenemez. Listede mevcut!!");
                }
                var addItem = new TblCustomer();
                addItem.CustomerName = item.CustomerName;
                _dbContext.TblCustomers.Add(addItem);
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                var customer = await _dbContext.TblCustomers.Where(p => p.Id == item.Id).FirstOrDefaultAsync();

                if (customer == null)
                {
                    throw new Exception("Müşteri bulunamadı");
                }

                customer.CustomerName = item.CustomerName;

                _dbContext.TblCustomers.Update(customer);
                await _dbContext.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> DeleteCustomer(string token, int id)
        {
            var customer = await _dbContext.TblCustomers.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (customer == null)
            {
                throw new Exception("Müşteri bulunamadı");
            }

            customer.DeletedTime = DateTime.Now;

            _dbContext.TblCustomers.Update(customer);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }

    public class CustomerItem
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
    }
}

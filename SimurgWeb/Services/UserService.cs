using Microsoft.EntityFrameworkCore;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace SimurgWeb.Services
{    
    public class UserService
    {
        private readonly SimurgContext _dbContext;

        public UserService(SimurgContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserItem>> GetUserList(string token)
        {
            return _dbContext.TblUsers.Select(p=> new UserItem{ 
                Id = p.Id,
                IsActive = p.IsActive,
                Password = "********",
                Username = p.Username
            }).ToList();
        }
        public async Task<bool> AddOrUpdateUser(string token, UserItem item)
        {
            if (item.Id == 0)
            {
                if (_dbContext.TblUsers.Any(p=>p.Username == item.Username))
                {
                    throw new Exception("Bu kullanıcı eklenemez. Listede mevcut!!");
                }

                var cyripto = new EncryptionHelper();
                var addItem = new TblUser();
                addItem.Username = item.Username;
                addItem.IsActive = item.IsActive;
                addItem.Password = cyripto.Encrypt(item.Password);
                _dbContext.TblUsers.Add(addItem);
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                var getUserId = _dbContext.TblUsers.FirstOrDefault(p => p.Username == GetUserName(token));

                var user = await _dbContext.TblUsers.Where(p => p.Id == item.Id).FirstOrDefaultAsync();

                if (getUserId == null)
                {
                    throw new Exception("Kullanıcı bulunamadı");
                }

                if (user == null)
                {
                    throw new Exception("Kullanıcı bulunamadı");
                }

                if (getUserId.Id != user.Id)
                {
                    throw new Exception("Farklı bir kullanıcıya işlem yapamazsınız.");
                }

                if (item.Username == null || item.Password == null)
                {
                    throw new Exception("Kullanıcı adı veya şifre boş olamaz");
                }

                if (item.Password != "********")
                {
                    var cyripto = new EncryptionHelper();
                    var newPss = cyripto.Encrypt(item.Password);
                    user.Password = newPss;
                }

                user.Username = item.Username;
                user.IsActive = item.IsActive;

                _dbContext.TblUsers.Update(user);
                await _dbContext.SaveChangesAsync();

                return true;
            }            
        }
        private string GetUserName(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault()?.Value;
        }
    }

    public class UserItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }
}

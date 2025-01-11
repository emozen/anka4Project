using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimurgWeb.SimurgModels;
using SimurgWeb.Utility;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimurgWeb.Services
{    
    public class AuthService
    {
        private readonly SimurgContext _dbContext;

        public AuthService(SimurgContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsLoggedIn { get; private set; } = false;

        public async Task<bool> OldLoginAsync(string userName, string password)
        {
            IsLoggedIn = await _dbContext.TblUsers.AnyAsync(p => p.Username == userName && p.Password == password);
            return IsLoggedIn;
        }

        public async Task<string> LoginAsync(string userName, string password)
        {
            var cyripto = new EncryptionHelper();
            var newPass = cyripto.Encrypt(password);
            IsLoggedIn = await _dbContext.TblUsers.AnyAsync(p => p.Username == userName && p.Password == newPass);
            if (IsLoggedIn)
            {
                return GenerateJwtToken(userName);
            }
            return "";
        }

        private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YourSuperSecretKey1234567890123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddMinutes(30), // Token süresi
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

using System.Text;
using System.Security.Claims;
using Ubee.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Ubee.Service.Exceptions;

namespace Ubee.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService userService;
        private readonly IConfiguration configuration;
        private readonly List<string> blacklistedTokens;

        public AuthService(IUserService userService, IConfiguration configuration)
        {
            this.userService = userService;
            this.configuration = configuration;
            this.blacklistedTokens = new List<string>();
        }

        public async ValueTask<string> GenerateTokenAsync(string username, string password)
        {
            var user = await this.userService.CheckUserAsync(username, password);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                     new Claim("Id", user.Id.ToString()),
                     //new Claim(ClaimTypes.Role, user.Role.ToString()),
                     new Claim(ClaimTypes.Name, user.Firstname)
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddSeconds(20),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async ValueTask BlacklistToken(string token)
        {
            blacklistedTokens.Add(token);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return blacklistedTokens.Contains(token);
        }

        public async ValueTask<bool> LogoutAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new CustomException(498, "Invalid token.");
            }

            await BlacklistToken(token);
            return true;
        }
    }
}

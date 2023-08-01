using System.Text;
using Ubee.Shared.Helpers;
using Ubee.Domain.Entities;
using System.Security.Claims;
using Ubee.Service.Interfaces;
using Ubee.Service.Exceptions;
using Ubee.Service.DTOs.Logins;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

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

        public async ValueTask<LoginResultDto> AuthenticateAsync(string username, string password)
        {
            var user = await userService.RetrieveByUserEmailAsync(username);
            if (user == null || !PasswordHelper.Verify(password, user.Password))
                throw new CustomException(400, "Email or password is incorrect");

            return new LoginResultDto
            {
                Token = GenerateToken(user)
            };
        }

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                 new Claim("Id", user.Id.ToString()),
                 new Claim(ClaimTypes.Name, user.Firstname)
                }),
                Audience = configuration["JWT:Audience"],
                Issuer = configuration["JWT:Issuer"],
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWT:Expire"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void BlacklistToken(string token)
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

            BlacklistToken(token);
            return true;
        }
    }
}

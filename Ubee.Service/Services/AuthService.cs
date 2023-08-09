using System.Text;
using Ubee.Shared.Helpers;
using Ubee.Domain.Entities;
using Ubee.Service.Helpers;
using System.Security.Claims;
using Ubee.Service.Interfaces;
using Ubee.Service.Exceptions;
using Ubee.Service.DTOs.Users;
using Ubee.Service.DTOs.Logins;
using Ubee.Service.DTOs.Phones;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Ubee.Service.Services
{
    public class AuthService : IAuthService
    {
        private const int CACHED_FOR_MINUTS_REGISTER = 60;
        private const int CACHED_FOR_MINUTS_VEFICATION = 5;

        private const string REGISTER_CACHE_KEY = "register_";
        private const string VERIFY_REGISTER_CACHE_KEY = "verify_register_";
        private const int VERIFICATION_MAXIMUM_ATTEMPTS = 3;

        private readonly IUserService userService;
        private readonly IMemoryCache memoryCache;
        private readonly IPhoneService phoneService;
        private readonly IConfiguration configuration;
        public AuthService(IUserService userService,
            IMemoryCache memoryCache,
            IPhoneService phoneService,
            IConfiguration configuration)
        {
            this.userService = userService;
            this.memoryCache = memoryCache;
            this.phoneService = phoneService;
            this.configuration = configuration;
        }

        public async ValueTask<(bool Result, int CachedMinutes)> SignUpAsync(UserForCreationDto dto)
        {
            var caostumerPhone = await this.userService.RetrieveByPhoneAsync(dto.Phone);
            if (caostumerPhone is not null)
                throw new CustomException(409, "User already exist.");

            if (this.memoryCache.TryGetValue(REGISTER_CACHE_KEY + dto.Phone, out UserForCreationDto registrDto))
            {
                registrDto.Phone = registrDto.Phone;
                this.memoryCache.Remove(dto.Phone);
            }
            else
                this.memoryCache.Set(
                    REGISTER_CACHE_KEY + dto.Phone, dto, TimeSpan.FromMinutes(CACHED_FOR_MINUTS_REGISTER));

            return (Result: true, CachedMinutes: CACHED_FOR_MINUTS_REGISTER);
        }

        public async ValueTask<(bool Result, int CachedVerificationMinutes)> SendCodeForSignUpAsync(string phone)
        {
            if (this.memoryCache.TryGetValue(REGISTER_CACHE_KEY + phone, out UserForCreationDto registrDto))
            {
                VerificationDto verificationDto = new VerificationDto();
                verificationDto.Attempt = 0;
                verificationDto.CreatedAt = DateTime.UtcNow;
                verificationDto.Code = CodeGenerator.GenerateRandomCode();
                this.memoryCache.Set(phone, verificationDto, TimeSpan.FromMinutes(CACHED_FOR_MINUTS_VEFICATION));

                // emal sende begin
                if (this.memoryCache.TryGetValue(VERIFY_REGISTER_CACHE_KEY + phone, out VerificationDto oldVerifcationDto))
                {
                    this.memoryCache.Remove(VERIFY_REGISTER_CACHE_KEY + phone);
                }
                this.memoryCache.Set(VERIFY_REGISTER_CACHE_KEY + phone, verificationDto,
                    TimeSpan.FromMinutes(CACHED_FOR_MINUTS_VEFICATION));

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpClient client = new HttpClient(clientHandler);

                PhoneMessage phoneMessage = new PhoneMessage();
                phoneMessage.Title = "Ubee";
                phoneMessage.Content = "Your verification code : " + verificationDto.Code;
                phoneMessage.Phone = phone.Substring(1);
                var result = await this.phoneService.SendMessageAsync(phoneMessage);
                if (result is true)
                    return (Result: true, CachedVerificationMinutes: CACHED_FOR_MINUTS_VEFICATION);
                else
                    return (Result: false, CACHED_FOR_MINUTS_VEFICATION: 0);
            }
            else
                throw new CustomException(489, "Verification expired token.");
        }

        public async ValueTask<(bool Result, string Token)> VerifySignUpAsync(string phone, int code)
        {
            if (this.memoryCache.TryGetValue(REGISTER_CACHE_KEY + phone, out UserForCreationDto registerDto))
            {
                if (this.memoryCache.TryGetValue(VERIFY_REGISTER_CACHE_KEY + phone, out VerificationDto verificationDto))
                {
                    if (verificationDto.Attempt >= VERIFICATION_MAXIMUM_ATTEMPTS)
                        throw new CustomException(429, "Verification too many requests.");
                    else if (verificationDto.Code == code)
                    {
                        var dbResult = await this.userService.AddUserAsync(registerDto);
                        var result = await this.userService.RetrieveByPhoneAsync(phone);

                        if (result is null) throw new CustomException(404, "User not found.");

                        string token = this.GenerateToken(result);

                        return (Result: dbResult != null, Token: token);
                    }
                    else
                    {
                        this.memoryCache.Remove(VERIFY_REGISTER_CACHE_KEY + phone);
                        verificationDto.Attempt++;
                        this.memoryCache.Set(VERIFY_REGISTER_CACHE_KEY + phone, verificationDto,
                            TimeSpan.FromMinutes(CACHED_FOR_MINUTS_VEFICATION));
                        return (Result: false, Token: "");
                    }
                }
                else throw new CustomException(489, "Verification code expired token.");
            }
            else throw new CustomException(489, "Verification expired token.");
        }

        public async ValueTask<LoginResultDto> AuthenticateAsync(string phone, string password)
        {
            var user = await this.userService.RetrieveByPhoneAsync(phone);
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
    }
}

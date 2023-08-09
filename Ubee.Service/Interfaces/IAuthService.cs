using Ubee.Service.DTOs.Logins;
using Ubee.Service.DTOs.Users;

namespace Ubee.Service.Interfaces
{
    public interface IAuthService
    {
        ValueTask<(bool Result, int CachedMinutes)> SignUpAsync(UserForCreationDto dto);
        ValueTask<(bool Result, int CachedVerificationMinutes)> SendCodeForSignUpAsync(string phone);
        ValueTask<(bool Result, string Token)> VerifySignUpAsync(string phone, int code);
        ValueTask<LoginResultDto> AuthenticateAsync(string username, string password);
    }
}

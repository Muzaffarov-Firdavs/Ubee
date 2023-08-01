using Ubee.Service.DTOs.Logins;

namespace Ubee.Service.Interfaces
{
    public interface IAuthService
    {
        ValueTask<LoginResultDto> AuthenticateAsync(string username, string password);
        ValueTask<bool> LogoutAsync(string token);
    }
}

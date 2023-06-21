using Ubee.Service.DTOs.Logins;

namespace Ubee.Service.Interfaces
{
    public interface IAuthService
    {
        ValueTask<string> GenerateTokenAsync(string username, string password);
    }
}

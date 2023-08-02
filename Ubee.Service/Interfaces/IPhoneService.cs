using Ubee.Service.DTOs.Phones;

namespace Ubee.Service.Interfaces
{
    public interface IPhoneService
    {
        Task<bool> SendMessageAsync(PhoneMessage phoneMessage);
    }
}

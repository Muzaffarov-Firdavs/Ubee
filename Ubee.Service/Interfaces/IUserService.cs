using Ubee.Service.DTOs.Users;
using Ubee.Domain.Configurations;
using Ubee.Domain.Entities;

namespace Ubee.Service.Interfaces;

public interface IUserService 
{
    ValueTask<bool> RemoveUserAsync(long id);
    ValueTask<UserForResultDto> RetrieveUserByIdAsync(long id);
    ValueTask<UserForResultDto> ModifyUserAsync(UserForUpdateDto userForUpdateDto);
    ValueTask<UserForResultDto> AddUserAsync(UserForCreationDto userForCreationDto);
    ValueTask<IEnumerable<UserForResultDto>> RetrieveAllUserAsync(PaginationParams @params, string search = null);
    ValueTask<UserForResultDto> CheckUserAsync(string username, string password = null);
    ValueTask<User> RetrieveByUserEmailAsync(string username);
}

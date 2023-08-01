using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ubee.Data.IRepositories;
using Ubee.Domain.Configurations;
using Ubee.Domain.Entities;
using Ubee.Service.DTOs.Users;
using Ubee.Service.Exceptions;
using Ubee.Service.Extensions;
using Ubee.Service.Interfaces;
using Ubee.Shared.Helpers;

namespace Ubee.Service.Services;

public class UserService : IUserService
{

    private readonly IMapper mapper;
    private readonly IRepository<User> userRepository;

    public UserService(IMapper mapper, IRepository<User> userRepository)
    {
        this.mapper = mapper;
        this.userRepository = userRepository;
    }
    public async ValueTask<UserForResultDto> AddUserAsync(UserForCreationDto dto)
    {
        var user = await this.userRepository.SelectAsync(u =>
                u.Username.ToLower() == dto.Username.ToLower() || u.Phone == dto.Phone);
        if (user is not null && !user.IsDeleted)
            throw new CustomException(409, "User is already exists");

        var mappedUser = this.mapper.Map<User>(dto);
        mappedUser.CreatedAt = DateTime.UtcNow;
        mappedUser.Password = PasswordHelper.Hash(dto.Password);
        var result = await this.userRepository.InsertAsync(mappedUser);
        await this.userRepository.SaveAsync();

        return this.mapper.Map<UserForResultDto>(result);
    }

    public async ValueTask<bool> RemoveUserAsync(long id)
    {
        var result = await this.userRepository.DeleteAysnyc(u => u.Id == id);
        if (!result)
            throw new CustomException(404, "User is not found");
        await this.userRepository.SaveAsync();

        return result;
    }


    public async ValueTask<IEnumerable<UserForResultDto>> RetrieveAllUserAsync(PaginationParams @params, string search = null)
    {
        var users = await this.userRepository.SelectAll(u => !u.IsDeleted)
            .ToPagedList(@params).ToListAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            users = users.FindAll(u => u.Username.ToLower().Contains(search));
        }
        return this.mapper.Map<IEnumerable<UserForResultDto>>(users);
    }

    public async ValueTask<UserForResultDto> RetrieveUserByIdAsync(long id)
    {
        var user = await this.userRepository.SelectAsync(u => u.Id == id && !u.IsDeleted);
        if (user is null)
            throw new CustomException(404, "User is not found ");
        var result = this.mapper.Map<UserForResultDto>(user);

        return result;
    }

    public async ValueTask<UserForResultDto> ModifyUserAsync(UserForUpdateDto dto)
    {
        var user = await this.userRepository.SelectAsync(u => u.Id == dto.Id && !u.IsDeleted);
        if (user is null)
            throw new CustomException(404, "User is not found ");

        var result = this.mapper.Map(dto, user);
        result.UpdatedAt = DateTime.UtcNow;
        await this.userRepository.SaveAsync();

        return this.mapper.Map<UserForResultDto>(result);
    }


    public async ValueTask<UserForResultDto> CheckUserAsync(string username, string password = null)
    {
        var user = await this.userRepository.SelectAsync(t => t.Username.ToLower().Equals(username.ToLower()));
        if (user is null || user.IsDeleted)
            throw new CustomException(404, "User is not found");
        return this.mapper.Map<UserForResultDto>(user);
    }

    public async ValueTask<User> RetrieveByUserEmailAsync(string username)
    {
        var user = await userRepository.SelectAsync(u => u.Username.ToLower() == username.ToLower());
        if (user is null || user.IsDeleted)
            throw new CustomException(404, "User Not Found");

        return user;
    }
}

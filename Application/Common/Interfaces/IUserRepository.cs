using server.Application.DTOs;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User entity);
        Task<bool> DeleteItemAsync(Guid id);
        Task<bool> DeleteItemsAsync(params Guid[] ids);
        Task<CursorPaginatedResult<UserModel>> GetAllAsync(UserSearch request);
        Task<User_Permisson_Dto> GetUserWithPermissionAsync(Guid id);
        Task<User> GetByIdAsync(Guid id);
        Task<bool> UpdateItemAsync(Guid id, User entity);
        Task<User> RegisterUser(User entity);
        Task UpdateLoginTime(Guid id, string accessToken);
        Task<User> GetEmailAsync(string email);
        Task<UserRolesAndPermissions> GetUserRolesAndPermissionsAsync(Guid userId);
        Task<bool> SetPassword(Guid userId, string newPassword);
        Task ClearToken(Guid id);
        Task<string> UpdateAvatar(Guid userId, IFormFile file);
    }
}
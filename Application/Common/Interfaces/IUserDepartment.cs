using System;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IUserDepartment : IRepository<UserDepartment>
{
    Task<CursorPaginatedResult<UserDepartmentModel>> GetDepartmentPageAsync(UserDepartmentSearch request);
    Task<UserDepartmentModel?> GetDetailByIdAsync(Guid id);
}

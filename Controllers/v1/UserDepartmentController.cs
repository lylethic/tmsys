using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Services;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/userDepartments")]
public class UserDepartmentController : BaseApiController
{
    private readonly IUserDepartment _repo;
    private readonly NotificationService _notificationService;
    public UserDepartmentController(IUserDepartment userDepartmentRepo,
        NotificationService notificationService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        this._notificationService = notificationService;
        this._repo = userDepartmentRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByID(Guid id)
    {
        try
        {
            var result = await _repo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GeDetailtByID(Guid id)
    {
        try
        {
            var result = await _repo.GetDetailByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get list of user-department relationships (cursor pagination)
    /// </summary>
    /// <remarks>
    /// This API supports filtering and cursor-based pagination.
    /// </remarks>
    /// <param name="search">
    /// Query parameters for searching user-department.
    /// </param>
    /// <returns>
    /// Cursor paginated list of user-department.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(UserDepartmentSearch search)
    {
        try
        {
            var result = await _repo.GetDepartmentPageAsync(search);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] UpSertUserDepartment dto)
    {
        try
        {
            var mapped = _mapper.Map<UserDepartment>(dto);
            var result = await _repo.AddAsync(mapped);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        try
        {
            var result = await _repo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, UpSertUserDepartment dto)
    {
        try
        {
            var mapped = _mapper.Map<UserDepartment>(dto);
            var result = await _repo.UpdateItemAsync(id, mapped);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
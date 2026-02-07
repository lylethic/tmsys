using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/role-permissions")]
public class RolePermissionsController : BaseApiController
{
    private readonly IRolePermission _rolePermission;

    public RolePermissionsController(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger,
        IRolePermission rolePermission) : base(mapper, httpContextAccessor, logger)
    {
        this._rolePermission = rolePermission;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] BaseSearch request)
    {
        try
        {
            var result = await _rolePermission.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(IEnumerable<Role_permissions> request)
    {
        try
        {
            var result = await _rolePermission.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _rolePermission.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("id")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _rolePermission.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("id")]
    public async Task<IActionResult> UpdateAsync(Guid id, Role_permissions request)
    {
        try
        {
            var result = await _rolePermission.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

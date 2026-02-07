using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/roles")]
public class RolesController : BaseApiController
{
    private readonly IRoleRepository _roleRepo;

    public RolesController(
       IRoleRepository roleRepository,
       IMapper mapper,
       IHttpContextAccessor httpContextAccessor,
       ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _roleRepo = roleRepository;
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetAll([FromQuery] BaseSearch request)
    {
        try
        {
            var result = await _roleRepo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("id")]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _roleRepo.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Add(RoleDto dto)
    {
        try
        {
            var request = _mapper.Map<Role>(dto);
            var result = await _roleRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("id")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _roleRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("id")]
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> Update(Guid id, RoleDto dto)
    {
        try
        {
            var request = _mapper.Map<Role>(dto);
            request.Id = id;
            var result = await _roleRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

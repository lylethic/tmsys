using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/sys-menus")]
public class SysMenuController : BaseApiController
{
    private readonly ISysMenuRepository _repo;

    public SysMenuController(
        ISysMenuRepository repo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _repo = repo;
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetAll([FromQuery] BaseSearch request)
    {
        try
        {
            var result = await _repo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _repo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpGet("by-group/{menuGroupId:guid}")]
    public async Task<IActionResult> GetByGroup(Guid menuGroupId)
    {
        try
        {
            var result = await _repo.GetByGroupIdAsync(menuGroupId);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Create([FromBody] SysMenuCreateDto dto)
    {
        try
        {
            var entity = _mapper.Map<SysMenu>(dto);
            var result = await _repo.AddAsync(entity);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("SYS_ADMIN", "UPDATE")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SysMenuUpdateDto dto)
    {
        try
        {
            var entity = _mapper.Map<SysMenu>(dto);
            var result = await _repo.UpdateItemAsync(id, entity);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _repo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }
}

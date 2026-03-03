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
[Route("v{version:apiVersion}/menu-groups")]
public class MenuGroupController : BaseApiController
{
    private readonly IMenuGroupRepository _repo;

    public MenuGroupController(
        IMenuGroupRepository repo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BaseSearch request)
    {
        try
        {
            var result = await _repo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    /// <summary>Returns all groups as a flat list with parent_id for frontend tree building.</summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        try
        {
            var result = await _repo.GetTreeAsync();
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

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Create([FromBody] MenuGroupCreateDto dto)
    {
        try
        {
            var entity = _mapper.Map<MenuGroup>(dto);
            var result = await _repo.AddAsync(entity);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("SYS_ADMIN", "UPDATE")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MenuGroupUpdateDto dto)
    {
        try
        {
            var entity = _mapper.Map<MenuGroup>(dto);
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

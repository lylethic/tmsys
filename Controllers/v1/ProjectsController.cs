using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request.Search;
using server.Common.CoreConstans;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/projects")]
public class ProjectsController : BaseApiController
{
    private readonly IProjectRepository _projectRepo;

    public ProjectsController(
        IProjectRepository projectRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _projectRepo = projectRepository;
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetAll([FromQuery] ProjectSearch request)
    {
        try
        {
            var result = await _projectRepo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _projectRepo.GetExtendProjectByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Add new project
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="status">0=Peding, 1=In Progress, 3=Resolved, 4=Rejected</param>
    /// <returns></returns>
    [HttpPost]
    [RequirePermission("SYS_ADMIN", "SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Add([FromBody] ProjectCreate dto, CoreConstants.ProjectStatus status)
    {
        try
        {
            var request = _mapper.Map<Project>(dto);
            request.Status = status.ToString();
            var result = await _projectRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _projectRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [RequirePermission("SYS_ADMIN", "SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProjectUpdate dto)
    {
        try
        {
            var request = _mapper.Map<Project>(dto);
            var result = await _projectRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

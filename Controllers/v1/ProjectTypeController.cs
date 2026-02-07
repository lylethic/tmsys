using System;
using System.Web.Http.Routing;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/project-type")]
public class ProjectTypeController : BaseApiController
{
    private readonly IProjectTypeRepository _projectType;
    public ProjectTypeController(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger,
        IProjectTypeRepository projectType
    ) : base(mapper, httpContextAccessor, logger)
    {
        this._projectType = projectType;
        this._mapper = mapper;
    }
    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> CreateAsync(CreateProjectType permission)
    {
        try
        {
            var request = _mapper.Map<ProjectType>(permission);
            var result = await _projectType.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("id")]
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateProjectType permission)
    {
        try
        {
            var request = _mapper.Map<ProjectType>(permission);
            var result = await _projectType.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("id")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _projectType.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(BaseSearch request)
    {
        var result = await _projectType.GetAllAsync(request);
        return Success(result);
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _projectType.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

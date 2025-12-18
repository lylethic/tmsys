using System;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/progress-updates")]
public class ProgressUpdatesController : BaseApiController
{
    private readonly IProgressUpdateRepository _progressUpdateRepo;

    public ProgressUpdatesController(
        IProgressUpdateRepository progressUpdateRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _progressUpdateRepo = progressUpdateRepository;
    }

    [HttpGet]
    [RequirePermission("READ", "AM_READ")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _progressUpdateRepo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("READ", "AM_READ")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _progressUpdateRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    [RequirePermission("CREATE", "AM_CREATE")]
    public async Task<IActionResult> Add([FromBody] ProgressUpdateCreate dto)
    {
        try
        {
            var request = _mapper.Map<ProgressUpdate>(dto);
            var result = await _progressUpdateRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("DELETE", "AM_DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _progressUpdateRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [RequirePermission("EDIT", "AM_EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProgressUpdateUpdate dto)
    {
        try
        {
            var request = _mapper.Map<ProgressUpdate>(dto);
            var result = await _progressUpdateRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

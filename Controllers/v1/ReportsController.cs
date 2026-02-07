using System;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/reports")]
public class ReportsController : BaseApiController
{
    private readonly IReportRepository _reportRepo;

    public ReportsController(
        IReportRepository reportRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _reportRepo = reportRepository;
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN", "READ")]
    public async Task<IActionResult> GetAll([FromQuery] ReportSearch request)
    {
        try
        {
            var result = await _reportRepo.GetAllAsync(request);
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
            var result = await _reportRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Add([FromBody] ReportCreate dto)
    {
        try
        {
            var request = _mapper.Map<Report>(dto);
            var result = await _reportRepo.AddAsync(request);
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
            var result = await _reportRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ReportUpdate dto)
    {
        try
        {
            var request = _mapper.Map<Report>(dto);
            var result = await _reportRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

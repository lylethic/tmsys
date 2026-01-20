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

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/workSchedules")]
public class WorkScheduleController : BaseApiController
{
    private readonly IWorkSchedule _workScheduleRepo;

    public WorkScheduleController(
        IWorkSchedule workScheduleRepo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _workScheduleRepo = workScheduleRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _workScheduleRepo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("mentees")]
    public async Task<IActionResult> GetMentees([FromQuery] string mentorEmail, [FromQuery] DateTimeOffset? weekStart, [FromQuery] DateTimeOffset? weekEnd)
    {
        try
        {
            var result = await _workScheduleRepo.GetMenteesByMentorEmailAsync(mentorEmail, weekStart, weekEnd);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _workScheduleRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkScheduleDto dto)
    {
        try
        {
            var request = _mapper.Map<Work_schedule>(dto);
            var result = await _workScheduleRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WorkScheduleDto dto)
    {
        try
        {
            var request = _mapper.Map<Work_schedule>(dto);
            var result = await _workScheduleRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _workScheduleRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

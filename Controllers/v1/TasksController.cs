using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;
using server.Services;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/tasks")] // URL: /api/v1/tasks
public class TasksController : BaseApiController
{
    private readonly ITaskRepository _taskRepo;
    private readonly NotificationService _notificationService;

    public TasksController(
        ITaskRepository taskRepository,
        NotificationService notificationService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _taskRepo = taskRepository;
        this._notificationService = notificationService;
    }

    [HttpGet]
    [RequirePermission("READ", "AM_READ")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _taskRepo.GetAllAsync(request);
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
            var result = await _taskRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    [RequirePermission("CREATE", "AM_CREATE")]
    public async Task<IActionResult> Add([FromBody] TaskCreate dto)
    {
        try
        {
            var request = _mapper.Map<Tasks>(dto);
            var result = await _taskRepo.AddAsync(request);
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
            var result = await _taskRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [RequirePermission("EDIT", "AM_EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskUpdate dto)
    {
        try
        {
            var request = _mapper.Map<Tasks>(dto);
            var result = await _taskRepo.UpdateItemAsync(id, request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("notiTask")]
    public async Task<IActionResult> GetNotiTask()
    {
        try
        {
            await _notificationService.CheckAndUpdateTaskNotificationsAsync();
            return Success(null);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

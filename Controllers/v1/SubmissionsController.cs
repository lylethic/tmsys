using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/submissions")]
public class SubmissionsController : BaseApiController
{
    private readonly ISubmissionRepository _submissionRepo;

    public SubmissionsController(
        ISubmissionRepository submissionRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _submissionRepo = submissionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SubmissionSearch request)
    {
        try
        {
            var result = await _submissionRepo.GetSubmissionPageAsync(request);
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
            var result = await _submissionRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("detail/{id}")]
    public async Task<IActionResult> GetSubmissionDetail(Guid id)
    {
        try
        {
            var result = await _submissionRepo.GetSubmissionAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Create([FromBody] SubmissionCreate dto)
    {
        try
        {
            var request = _mapper.Map<Submission>(dto);
            var result = await _submissionRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Submit a task with automatic scoring logic
    /// - Validates allow_late, allow_resubmit
    /// - Automatically calculates is_late, penalty_point
    /// - Status set to "Pending Review" awaiting leader approval
    /// </summary>
    [HttpPost("submit")]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> SubmitTask([FromBody] SubmitTaskRequest dto)
    {
        try
        {
            var result = await _submissionRepo.SubmitTaskAsync(
                dto.Task_id,
                dto.User_id,
                dto.Raw_point,
                dto.Note
            );
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Leader reviews and approves/rejects a submission
    /// - Updates raw_point based on leader's assessment
    /// - Recalculates final_score and determines is_pass
    /// - Updates task status to Completed if approved and passed
    /// </summary>
    [HttpPost("{id}/review")]
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> ReviewSubmission(Guid id, [FromBody] ReviewSubmissionRequest dto)
    {
        try
        {
            var reviewerId = Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token"));

            var result = await _submissionRepo.ReviewSubmissionAsync(
                id,
                reviewerId,
                dto.Raw_point,
                dto.Is_approved,
                dto.Review_note
            );
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubmissionUpdate dto)
    {
        try
        {
            var request = _mapper.Map<Submission>(dto);
            var result = await _submissionRepo.UpdateItemAsync(id, request);
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
            var result = await _submissionRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

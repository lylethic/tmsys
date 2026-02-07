using System;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IWorkSchedule : IRepository<Work_schedule>
{
    Task<CursorPaginatedResult<Work_schedule>> GetAllAsync(WorkScheduleSearch request);
    Task<CursorPaginatedResult<MenteeDto>> GetMenteesByMentorEmailAsync(string mentorEmail, CursorPaginationRequest request, DateTimeOffset? weekStart, DateTimeOffset? weekEnd);
}

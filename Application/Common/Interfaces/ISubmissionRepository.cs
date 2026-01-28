using System;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> AddAsync(Submission entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<CursorPaginatedResult<Submission>> GetSubmissionPageAsync(SubmissionSearch request);
    Task<Submission> GetByIdAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, Submission entity);
    Task<SubmissionModel> GetSubmissionAsync(Guid id);
    Task<Submission> SubmitTaskAsync(Guid taskId, Guid userId, decimal? rawPoint, string? note);
    Task<Submission> ReviewSubmissionAsync(Guid submissionId, Guid reviewerId, decimal rawPoint, bool isApproved, string? reviewNote);
}

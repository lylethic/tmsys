using System;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<Report> AddAsync(Report entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<PaginatedResult<Report>> GetAllAsync(PaginationRequest request);
    Task<Report> GetByIdAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, Report entity);
    Task<PaginatedResult<ReportModel>> GetAllAsync(ReportSearch request);
}

using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<CursorPaginatedResult<Report>> GetAllAsync(ReportSearch request);
}

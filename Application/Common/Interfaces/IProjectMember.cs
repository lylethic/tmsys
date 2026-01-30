using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IProjectMember : IRepository<ProjectMember>
{
    Task<CursorPaginatedResult<ProjectMemberModel>> GetProjectMemberPageAsync(ProjectMemberSearch request);
    Task<ProjectMemberModel> GetProjectMemberAsync(Guid id);
}

using server.Application.Request;
using server.Application.Request.Search;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces
{
    public interface IDepartment : IRepository<Department>
    {
        Task<CursorPaginatedResult<Department>> GetDepartmentPageAsync(DepartmentSearch request);
    }
}

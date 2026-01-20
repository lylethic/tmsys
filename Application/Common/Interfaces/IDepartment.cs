using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces
{
    public interface IDepartment : IRepository<Department>
    {
        Task<CursorPaginatedResult<DepartmentTreeDto>> GetDepartmentPageAsync(DepartmentSearch request);
        Task<IEnumerable<Department>> AddAsync(List<Department> departments);
    }
}

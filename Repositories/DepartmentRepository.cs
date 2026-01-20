using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;
using Sprache;
using System.Data;
using System.Linq;

namespace server.Repositories
{
    public class DepartmentRepository : SimpleCrudRepository<Department, Guid>, IDepartment
    {
        private readonly IAssistantService _assistantService;

        public DepartmentRepository(IDbConnection connection, IAssistantService assistantService) : base(connection)
        {
            _connection = connection;
            _assistantService = assistantService;
        }

        public async Task<Department> AddAsync(Department entity)
        {
            entity.Id = Uuid7.NewUuid7().ToGuid();
            entity.Created_by = Guid.Parse(_assistantService.UserId);
            var sql = """
                INSERT INTO public.departments 
                    (id, code, name, description, parent_id, 
                    created, updated, created_by, updated_by, deleted, active)
                VALUES (
                    @Id, @Code, @Name, @Description, @Parent_id, 
                    @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active)
            """;
            try
            {
                var query = await _connection.ExecuteAsync(sql, entity);
                if (query > 0)
                {
                    var result = await GetByIdAsync(entity.Id);
                    if (result != null)
                        return result;
                    throw new BadRequestException("Department created, but failed to retrieve it.");
                }
                throw new BadRequestException("Failed to insert department into the database");
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        public async Task<IEnumerable<Department>> AddAsync(List<Department> departments)
        {
            var userId = Guid.Parse(_assistantService.UserId);
            var now = DateTime.UtcNow;

            foreach (var dept in departments)
            {
                dept.Id = Uuid7.NewUuid7().ToGuid();
                dept.Code = dept.Code.Trim().ToUpperInvariant();
                dept.Created = now;
                dept.Updated = now;
                dept.Created_by = userId;
                dept.Updated_by = userId;
                dept.Deleted = false;
                dept.Active = true;
            }

            var sql = """
                INSERT INTO public.departments
                (id, code, name, description, parent_id,
                created, updated, created_by, updated_by, deleted, active)
                VALUES
                (@Id, @Code, @Name, @Description, @Parent_id,
                @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active)
            """;

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            using var transaction = _connection.BeginTransaction();


            try
            {
                await _connection.ExecuteAsync(sql, departments, transaction);
                transaction.Commit();

                return departments;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            await base.SoftDeleteAsync(id);
            return true;
        }

        public async Task<CursorPaginatedResult<DepartmentTreeDto>> GetDepartmentPageAsync(DepartmentSearch request)
        {
            var where = new List<string>();
            var param = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                where.Add("code ILIKE '%' || @Keyword || '%' OR name ILIKE '%' || @Keyword || '%'");
                param.Add("Keyword", request.Keyword);
            }
            var result = await GetListByIdCursorNoDeleleColAsync<Department>(
                request: request,
                extraWhere: string.Join(" AND ", where),
                extraParams: param
            );

            var rootIds = result.Data.Select(d => d.Id).ToList();
            if (rootIds.Count == 0)
            {
                return new CursorPaginatedResult<DepartmentTreeDto>
                {
                    Data = new List<DepartmentTreeDto>(),
                    NextCursor = result.NextCursor,
                    NextCursorSortOrder = result.NextCursorSortOrder,
                    HasNextPage = result.HasNextPage,
                    Total = result.Total
                };
            }

            var allDepartments = await GetDepartmentsWithDescendantsAsync(rootIds);
            var treeData = BuildDepartmentTree(allDepartments, rootIds);

            return new CursorPaginatedResult<DepartmentTreeDto>
            {
                Data = treeData,
                NextCursor = result.NextCursor,
                NextCursorSortOrder = result.NextCursorSortOrder,
                HasNextPage = result.HasNextPage,
                Total = result.Total
            };
        }

        private async Task<List<Department>> GetDepartmentsWithDescendantsAsync(List<Guid> rootIds)
        {
            var sql = $"""
                WITH RECURSIVE dept_tree AS (
                    SELECT *, ARRAY[id] AS path
                    FROM {_dbTableName}
                    WHERE id = ANY(@RootIds) AND deleted = false
                    UNION ALL
                    SELECT d.*, dt.path || d.id
                    FROM {_dbTableName} d
                    INNER JOIN dept_tree dt ON d.parent_id = dt.id
                    WHERE d.deleted = false
                      AND NOT d.id = ANY(dt.path)
                )
                SELECT *
                FROM dept_tree;
            """;

            var result = await _connection.QueryAsync<Department>(sql, new { RootIds = rootIds.ToArray() });
            return result.ToList();
        }

        private static List<DepartmentTreeDto> BuildDepartmentTree(List<Department> departments, List<Guid> rootIds)
        {
            var map = departments
                .GroupBy(d => d.Id)
                .Select(g => g.First())
                .ToDictionary(
                    d => d.Id,
                    d => new DepartmentTreeDto
                    {
                        Id = d.Id,
                        Code = d.Code,
                        Name = d.Name,
                        Description = d.Description,
                        Parent_id = d.Parent_id,
                        Active = d.Active,
                        Deleted = d.Deleted,
                        Created = d.Created,
                        Updated = d.Updated,
                        Created_by = d.Created_by,
                        Updated_by = d.Updated_by
                    });

            foreach (var node in map.Values)
            {
                if (!node.Parent_id.HasValue)
                    continue;

                if (node.Parent_id.Value == node.Id)
                    continue;

                if (map.TryGetValue(node.Parent_id.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
            }

            var roots = new List<DepartmentTreeDto>();
            foreach (var rootId in rootIds)
            {
                if (!map.TryGetValue(rootId, out var rootNode))
                    continue;

                if (rootNode.Parent_id.HasValue &&
                    rootNode.Parent_id.Value != rootNode.Id &&
                    map.ContainsKey(rootNode.Parent_id.Value))
                {
                    continue;
                }

                if (roots.Any(r => r.Id == rootNode.Id))
                    continue;

                roots.Add(rootNode);
            }

            return roots;
        }

        public async Task<bool> UpdateItemAsync(Guid id, Department entity)
        {
            var existing = await base.GetByIdAsync(id)
                ?? throw new NotFoundException("Department not found!");
            entity.Id = id;
            var sql = """
                UPDATE public.departments
                SET 
                    code = @Code,
                    name = @Name,
                    description = @Description,
                    parent_id = @Parent_id,
                    active = @Active,
                    updated = @Updated,
                    updated_by = @Updated_by
                WHERE id = @Id;
            """;
            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0) return true;
            throw new BadRequestException("Failed to update department");
        }
    }
}

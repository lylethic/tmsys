using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;
using Sprache;
using System.Data;

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

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            await base.SoftDeleteAsync(id);
            return true;
        }

        public async Task<CursorPaginatedResult<Department>> GetDepartmentPageAsync(DepartmentSearch request)
        {
            var where = new List<string>();
            var param = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                where.Add("code ILIKE '%' || @Keyword || '%' OR name ILIKE '%' || @Keyword || '%'");
                param.Add("Keyword", request.Keyword);
            }
            return await GetListByIdCursorNoDeleleColAsync<Department>(
                request: request,
                extraWhere: string.Join(" AND ", where),
                extraParams: param
            );
        }

        public async Task<bool> UpdateItemAsync(Guid id, Department entity)
        {
            var existing = await base.GetByIdAsync(id)
                ?? throw new NotFoundException("Department not found!");
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
            """;
            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0) return true;
            throw new BadRequestException("Failed to update department");
        }
    }
}

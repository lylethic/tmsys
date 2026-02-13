using System.Data;
using Dapper;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Application.Search;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class RolePermissionRepository(IDbConnection connection) : SimpleCrudRepository<Role_permissions, Guid>(connection), IRolePermission
{
    public async Task<IEnumerable<Role_permissions>> AddAsync(IEnumerable<Role_permissions> entity)
    {
        if (entity is null || !entity.Any())
            throw new BadRequestException("Please enter your role permission.");

        const string sql = """
            INSERT INTO role_permissions (role_id, permission_id) 
            VALUES (@Role_id, @Permission_id)
        """;

        try
        {
            var result = await _connection.ExecuteAsync(sql, entity);
            if (result == 0) throw new BadRequestException("Failed to insert role permission into the database.");

            return entity;
            throw new BadRequestException("Role permission created, but failed to retrieve it.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<Role_permissions> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM role_permissions WHERE permission_id = @Id";
        var result = await _connection.QuerySingleOrDefaultAsync<Role_permissions>(sql, new { Id = id })
            ?? throw new NotFoundException("Role permission not found.");
        return result;
    }

    public async Task<CursorPaginatedResult<Role_permissions>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();
        where.Add("deleted = false");
        if (request.Keyword is not null)
        {
            where.Add("(CAST(role_id AS TEXT) ILIKE @Keyword OR CAST(permission_id AS TEXT) ILIKE @Keyword)");
            param.Add("Keyword", $"%{request.Keyword}%");
        }

        return await this.GetListCursorBasedAsync<Role_permissions>(
           request: request,
           extraWhere: string.Join(" AND ", where),
           extraParams: param,
           orderDirection: request.Ascending ? "ASC" : "DESC",
           idColumn: "id"
         );
    }

    public Task<Role_permissions> AddAsync(Role_permissions entity)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        try
        {
            var existingRolePermission = await GetByIdAsync(id)
                ?? throw new NotFoundException("Role permission not found.");

            const string sql = "DELETE FROM role_permissions WHERE id = @Id";
            var result = await _connection.ExecuteAsync(sql, new { Id = id });

            if (result == 0)
                throw new BadRequestException("Failed to delete role permission.");

            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> UpdateItemAsync(Guid id, Role_permissions entity)
    {
        try
        {
            var existingRolePermission = await GetByIdAsync(id)
                ?? throw new NotFoundException("User's role not found.");

            const string sql = """
                UPDATE role_permissions
                SET permission_id = @Permission_id
                WHERE role_id = @Role_id;
            """;
            await _connection.ExecuteAsync(sql, entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

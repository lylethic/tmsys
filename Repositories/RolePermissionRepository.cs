using System.Data;
using Dapper;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
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

    public Task<Role_permissions> AddAsync(Role_permissions entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Role_permissions> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM role_permissions WHERE permission_id = @Id";
        var result = await _connection.QuerySingleOrDefaultAsync<Role_permissions>(sql, new { Id = id })
            ?? throw new NotFoundException("Role permission not found.");
        return result;
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

    public async Task<PaginatedResult<Role_permissions>> GetAllAsync(PaginationRequest request)
    {
        const string sql = """
            SELECT * FROM role_permissions
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM role_permissions;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Role_permissions>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Role_permissions>
            {
                Data = result.Count > 0 ? result : [],
                TotalCount = totalRecords,
                Page = request.PageIndex,
                PageSize = request.PageSize
            };
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

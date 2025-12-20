using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using System.Data;

namespace server.Repositories;

public class PermissionRepository : SimpleCrudRepository<Permission, Guid>, IPermissionRepository
{
    public PermissionRepository(IDbConnection connection) : base(connection)
    {

    }
    public async Task<PaginatedResult<Permission>> GetPermissionsAsync(PaginationRequest request)
    {
        const string sql = """
            SELECT id, name from permissions
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM permissions;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Permission>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Permission>
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

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        const string sql = """
            SELECT DISTINCT p.id, p.name
            FROM permissions p
                INNER JOIN role_permissions rp ON p.id = rp.permission_id
                INNER JOIN roles r ON rp.role_id = r.id
                INNER JOIN user_roles ar ON r.id = ar.role_id
            WHERE ar.user_id = @UserId
        """;

        var permissions = await _connection.QueryAsync<Permission>(sql, new { UserId = userId });
        return [.. permissions];
    }

    public async Task<List<Role>> GetUserRolesAsync(Guid userId)
    {
        const string sql = """
            SELECT r.id, r.name
            FROM roles r
                INNER JOIN user_roles ar ON r.id = ar.role_id
            WHERE ar.user_id = @UserId
        """;

        var roles = await _connection.QueryAsync<Role>(sql, new { UserId = userId });
        return [.. roles];
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM permissions p
                INNER JOIN role_permissions rp ON p.id = rp.permission_id
                INNER JOIN roles r ON rp.role_id = r.id
                INNER JOIN user_roles ar ON r.id = ar.role_id
            WHERE ar.user_id = @UserId AND p.name = @PermissionName
        """;

        var count = await _connection.QuerySingleAsync<int>(sql, new { UserId = userId, PermissionName = permissionName });
        return count > 0;
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        const string sql = $"""
            SELECT COUNT(1)
            FROM roles r
                INNER JOIN user_roles ar ON r.id = ar.role_id
            WHERE ar.user_id = @UserId AND r.name = @RoleName
        """;

        var count = await _connection.QuerySingleAsync<int>(sql, new { UserId = userId, RoleName = roleName });
        return count > 0;
    }

    public async Task<User> GetUserWithRolesAndPermissionsAsync(Guid userId)
    {
        const string sql = """
            SELECT a.id, a.email, a.school_id, a.active, a.created, a.updated,
                   r.id, r.name,
                   p.id, p.name
            FROM users a
                LEFT JOIN user_roles ar ON a.id = ar.user_id
                LEFT JOIN roles r ON ar.role_id = r.id
                LEFT JOIN role_permissions rp ON r.id = rp.role_id
                LEFT JOIN permissions p ON rp.permission_id = p.id
            WHERE a.id = @userId
        """;

        var userDict = new Dictionary<Guid, User>();
        var roleDict = new Dictionary<Guid, Role>();

        await _connection.QueryAsync<User, Role, Permission, User>(
            sql,
            (user, role, permission) =>
            {
                if (!userDict.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userEntry.Role_id = role.Id;
                    userEntry.Permissions = [];
                    userDict.Add(user.Id, userEntry);
                }

                if (role != null)
                {
                    if (!roleDict.TryGetValue(role.Id, out var roleEntry))
                    {
                        roleEntry = role;
                        roleDict.Add(role.Id, roleEntry);
                    }

                    if (permission != null && !userEntry.Permissions.Any(p => p.Id == permission.Id))
                    {
                        userEntry.Permissions.Add(permission);
                    }
                }

                return userEntry;
            },
            new { userId = userId },
            splitOn: "id,id"
        );

        return userDict.Values.FirstOrDefault();
    }

    public async Task<Permission> AddAsync(Permission permission)
    {
        permission.Id = Uuid7.NewUuid7().ToGuid(); ;
        var sql = """
            INSERT INTO permissions (id, name) 
            VALUES (@Id, @Name)
        """;
        try
        {
            await _connection.ExecuteAsync(sql, permission);
            var inserted = await GetByIdAsync(permission.Id);
            if (inserted is not null)
                return inserted;
            throw new BadRequestException("Permission created, but failed to retrieve it.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<Permission> UpdateAsync(Guid id, Permission permission)
    {
        var sql = """
            UPDATE permissions
            SET name = @Name
            WHERE id = @Id
        """;

        try
        {
            var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id, permission.Name });
            if (affectedRows == 0)
                throw new BadRequestException("No permission found with the given ID.");
            var updated = await GetByIdAsync(id);
            if (updated is not null)
                return updated;
            throw new BadRequestException("Permission created, but failed to retrieve it.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<string> DeleteAsync(Guid id)
    {
        try
        {
            var checkSql = """
                SELECT 1 FROM permissions WHERE id = @Id
            """;

            var exists = await _connection.ExecuteScalarAsync<int?>(checkSql, new { Id = id });

            if (exists is null)
                throw new BadRequestException("Permission with the given ID does not exist.");

            var deleteSql = """
                DELETE FROM permissions
                WHERE id = @Id
            """;

            await _connection.ExecuteAsync(deleteSql, new { Id = id });

            return "Permission deleted successfully.";
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

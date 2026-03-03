using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class RoleMenuRepository(IDbConnection connection, ITransactionContext transactionContext)
    : SimpleCrudRepository<RoleMenu, Guid>(connection, transactionContext), IRoleMenuRepository
{
    public async Task<RoleMenu> AddAsync(RoleMenu entity)
    {
        if (entity is null)
            throw new BadRequestException("Role menu data is required.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created = DateTime.UtcNow;
        entity.Updated = DateTime.UtcNow;

        const string sql = """
            INSERT INTO role_menus
                (id, role_id, menu_id, controller_name,
                 can_view, can_create, can_update, can_delete, can_approve, can_export,
                 deleted, created, updated, created_by, updated_by)
            VALUES
                (@Id, @RoleId, @MenuId, @ControllerName,
                 @CanView, @CanCreate, @CanUpdate, @CanDelete, @CanApprove, @CanExport,
                 FALSE, @Created, @Updated, @Created_by, @Updated_by)
        """;

        try
        {
            var rows = await _connection.ExecuteAsync(sql, entity);
            if (rows > 0)
                return await GetByIdAsync(entity.Id)
                    ?? throw new BadRequestException("Created but failed to retrieve role menu.");

            throw new BadRequestException("Failed to insert role menu.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<RoleMenu> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT * FROM role_menus
            WHERE id = @Id AND deleted = FALSE
            LIMIT 1
        """;

        return await _connection.QuerySingleOrDefaultAsync<RoleMenu>(sql, new { Id = id })
            ?? throw new NotFoundException("Role menu not found.");
    }

    public async Task<bool> UpdateItemAsync(Guid id, RoleMenu entity)
    {
        try
        {
            _ = await GetByIdAsync(id);

            entity.Id = id;
            entity.Updated = DateTime.UtcNow;

            const string sql = """
                UPDATE role_menus
                SET can_view    = @CanView,
                    can_create  = @CanCreate,
                    can_update  = @CanUpdate,
                    can_delete  = @CanDelete,
                    can_approve = @CanApprove,
                    can_export  = @CanExport,
                    updated  = @Updated,
                    updated_by  = @Updated_by
                WHERE id = @Id AND deleted = FALSE
            """;

            await _connection.ExecuteAsync(sql, entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        try
        {
            _ = await GetByIdAsync(id);

            const string sql = """
                UPDATE role_menus
                SET deleted = TRUE, updated = NOW()
                WHERE id = @Id
            """;

            await _connection.ExecuteAsync(sql, new { Id = id });
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<CursorPaginatedResult<RoleMenu>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string> { "deleted = FALSE" };
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            where.Add("controller_name ILIKE @Keyword");
            param.Add("Keyword", $"%{request.Keyword}%");
        }

        return await GetListCursorBasedAsync<RoleMenu>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderDirection: request.Ascending ? "ASC" : "DESC",
            idColumn: "id"
        );
    }

    public async Task<List<RoleMenu>> GetByRoleIdAsync(Guid roleId)
    {
        const string sql = """
            SELECT rm.*, sm.name AS menu_name
            FROM role_menus rm
            LEFT JOIN sys_menus sm ON sm.id = rm.menu_id
            WHERE rm.role_id = @RoleId AND rm.deleted = FALSE
            ORDER BY sm.sort ASC
        """;

        var result = await _connection.QueryAsync<RoleMenu>(sql, new { RoleId = roleId });
        return result.ToList();
    }

    /// <summary>
    /// Returns the full dynamic menu tree for a role.
    /// Only includes groups and items where can_view = TRUE on the role_menus record.
    /// </summary>
    public async Task<List<DynamicMenuDto>> GetDynamicMenuAsync(Guid roleId)
    {
        const string sql = """
            SELECT
                mg.id           AS group_id,
                mg.name         AS group_name,
                mg.icon         AS group_icon,
                mg.sort         AS group_sort,
                mg.parent_id    AS parent_group_id,
                sm.id           AS menu_id,
                sm.name         AS menu_name,
                sm.controller   AS controller,
                sm.controller_name,
                sm.action,
                sm.sort,
                rm.can_view,
                rm.can_create,
                rm.can_update,
                rm.can_delete,
                rm.can_approve,
                rm.can_export
            FROM role_menus rm
            JOIN sys_menus  sm ON sm.id = rm.menu_id
            JOIN menu_groups mg ON mg.id = sm.menu_group_id
            WHERE rm.role_id   = @RoleId
              AND rm.can_view  = TRUE
              AND rm.deleted   = FALSE
              AND sm.active = TRUE
              AND sm.is_show_menu = TRUE
              AND sm.deleted   = FALSE
              AND mg.active = TRUE
              AND mg.deleted   = FALSE
            ORDER BY mg.sort ASC, sm.sort ASC
        """;

        var rows = await _connection.QueryAsync(sql, new { RoleId = roleId });

        var groupMap = new Dictionary<Guid, DynamicMenuDto>();

        foreach (var row in rows)
        {
            var gid = (Guid)row.group_id;

            if (!groupMap.TryGetValue(gid, out var group))
            {
                group = new DynamicMenuDto
                {
                    GroupId = gid,
                    GroupName = row.group_name,
                    GroupIcon = row.group_icon,
                    GroupSort = row.group_sort,
                    ParentGroupId = row.parent_group_id
                };
                groupMap[gid] = group;
            }

            group.Items.Add(new DynamicMenuItemDto
            {
                MenuId = row.menu_id,
                Name = row.menu_name,
                Controller = row.controller,
                ControllerName = row.controller_name,
                Action = row.action,
                Sort = row.sort,
                CanView = row.can_view,
                CanCreate = row.can_create,
                CanUpdate = row.can_update,
                CanDelete = row.can_delete,
                CanApprove = row.can_approve,
                CanExport = row.can_export
            });
        }

        return groupMap.Values.OrderBy(g => g.GroupSort).ToList();
    }

    /// <summary>
    /// Replaces all role_menus for a role in a single transaction.
    /// </summary>
    public async Task<bool> UpsertRoleMenusAsync(Guid roleId, List<RoleMenuCreateDto> permissions)
    {
        if (permissions is null || permissions.Count == 0)
            throw new BadRequestException("Permissions list is required.");

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        var ownedTx = _transactionContext?.Current == null ? _connection.BeginTransaction() : null;
        var tx = _transactionContext?.Current ?? ownedTx;
        try
        {
            // Soft-delete existing records for this role
            const string deleteSql = """
                UPDATE role_menus
                SET deleted = TRUE, updated = NOW()
                WHERE role_id = @RoleId AND deleted = FALSE
            """;
            await _connection.ExecuteAsync(deleteSql, new { RoleId = roleId }, tx);

            const string insertSql = """
                INSERT INTO role_menus
                    (id, role_id, menu_id, controller_name,
                     can_view, can_create, can_update, can_delete, can_approve, can_export,
                     deleted, created, updated)
                VALUES
                    (@Id, @RoleId, @MenuId, @ControllerName,
                     @CanView, @CanCreate, @CanUpdate, @CanDelete, @CanApprove, @CanExport,
                     FALSE, NOW(), NOW())
            """;

            var rows = permissions.Select(p => new
            {
                Id = Uuid7.NewUuid7().ToGuid(),
                p.RoleId,
                p.MenuId,
                p.ControllerName,
                p.CanView,
                p.CanCreate,
                p.CanUpdate,
                p.CanDelete,
                p.CanApprove,
                p.CanExport
            });

            await _connection.ExecuteAsync(insertSql, rows, tx);
            ownedTx?.Commit();
            return true;
        }
        catch (Exception ex)
        {
            ownedTx?.Rollback();
            throw new InternalErrorException(ex.Message);
        }
    }
}

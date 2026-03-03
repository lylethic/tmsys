using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Application.Search;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class SysMenuRepository(IDbConnection connection, ITransactionContext transactionContext)
    : SimpleCrudRepository<SysMenu, Guid>(connection, transactionContext), ISysMenuRepository
{
    public async Task<SysMenu> AddAsync(SysMenu entity)
    {
        if (entity is null)
            throw new BadRequestException("Menu item data is required.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created = DateTime.UtcNow;
        entity.Updated = DateTime.UtcNow;

        const string sql = """
            INSERT INTO sys_menus
                (id, controller_name, name_vn, controller, action, name, menu_group_id, sort,
                 can_view, can_create, can_update, can_delete, can_approve, can_export,
                 active, is_show_menu, deleted, created, updated, created_by, updated_by)
            VALUES
                (@Id, @ControllerName,  @Name_vn, @Controller, @Action, @Name, @MenuGroupId, @Sort,
                 @CanView, @CanCreate, @CanUpdate, @CanDelete, @CanApprove, @CanExport,
                 @Active, @IsShowMenu, FALSE, @Created, @Updated, @Created_by, @Updated_by)
        """;

        try
        {
            var rows = await _connection.ExecuteAsync(sql, entity);
            if (rows > 0)
                return await GetByIdAsync(entity.Id)
                    ?? throw new BadRequestException("Created but failed to retrieve menu item.");

            throw new BadRequestException("Failed to insert menu item.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<SysMenu> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT * FROM sys_menus
            WHERE id = @Id AND deleted = FALSE
            LIMIT 1
        """;

        return await _connection.QuerySingleOrDefaultAsync<SysMenu>(sql, new { Id = id })
            ?? throw new NotFoundException("Menu item not found.");
    }

    public async Task<bool> UpdateItemAsync(Guid id, SysMenu entity)
    {
        try
        {
            _ = await GetByIdAsync(id);

            entity.Id = id;
            entity.Updated = DateTime.UtcNow;

            const string sql = """
                UPDATE sys_menus
                SET controller_name = @ControllerName,
                    controller      = @Controller,
                    action          = @Action,
                    name            = @Name,
                    name_vn       = @Name_vn,
                    menu_group_id   = @MenuGroupId,
                    sort            = @Sort,
                    can_view        = @CanView,
                    can_create      = @CanCreate,
                    can_update      = @CanUpdate,
                    can_delete      = @CanDelete,
                    can_approve     = @CanApprove,
                    can_export      = @CanExport,
                    active       = @Active,
                    is_show_menu    = @IsShowMenu,
                    updated      = @Updated,
                    updated_by      = @Updated_by
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
                UPDATE sys_menus
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

    public async Task<CursorPaginatedResult<SysMenu>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string> { "deleted = FALSE" };
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            where.Add("(name ILIKE @Keyword OR controller_name ILIKE @Keyword OR controller ILIKE @Keyword)");
            param.Add("Keyword", $"%{request.Keyword}%");
        }

        return await GetListCursorBasedAsync<SysMenu>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderDirection: request.Ascending ? "ASC" : "DESC",
            idColumn: "id"
        );
    }

    public async Task<List<SysMenu>> GetByGroupIdAsync(Guid menuGroupId)
    {
        const string sql = """
            SELECT * FROM sys_menus
            WHERE menu_group_id = @MenuGroupId
              AND active = TRUE
              AND deleted = FALSE
            ORDER BY sort ASC
        """;

        var result = await _connection.QueryAsync<SysMenu>(sql, new { MenuGroupId = menuGroupId });
        return result.ToList();
    }
}

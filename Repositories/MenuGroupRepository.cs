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

public class MenuGroupRepository(IDbConnection connection, ITransactionContext transactionContext)
    : SimpleCrudRepository<MenuGroup, Guid>(connection, transactionContext), IMenuGroupRepository
{
    public async Task<MenuGroup> AddAsync(MenuGroup entity)
    {
        if (entity is null)
            throw new BadRequestException("Menu group data is required.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created = DateTime.UtcNow;
        entity.Updated = DateTime.UtcNow;

        var sql = """
            INSERT INTO menu_groups
                (id, name, name_vn, sort, icon, active, parent_id, deleted, created, updated, created_by, updated_by)
            VALUES
                (@Id, @Name, @Name_vn, @Sort, @Icon, @Active, @ParentId, FALSE, @Created, @Updated, @Created_by, @Updated_by)
        """;

        try
        {
            var rows = await _connection.ExecuteAsync(sql, entity);
            if (rows > 0)
                return await GetByIdAsync(entity.Id)
                    ?? throw new BadRequestException("Created but failed to retrieve menu group.");

            throw new BadRequestException("Failed to insert menu group.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<MenuGroup> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT * FROM menu_groups
            WHERE id = @Id AND deleted = FALSE
            LIMIT 1
        """;

        return await _connection.QuerySingleOrDefaultAsync<MenuGroup>(sql, new { Id = id })
            ?? throw new NotFoundException("Menu group not found.");
    }

    public async Task<bool> UpdateItemAsync(Guid id, MenuGroup entity)
    {
        try
        {
            _ = await GetByIdAsync(id);

            entity.Id = id;
            entity.Updated = DateTime.UtcNow;

            const string sql = """
                UPDATE menu_groups
                SET name       = @Name,
                    name_vn    = @Name_vn,
                    sort       = @Sort,
                    icon       = @Icon,
                    active  = @Active,
                    parent_id  = @ParentId,
                    updated = @Updated,
                    updated_by = @Updated_by
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
                UPDATE menu_groups
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

    public async Task<CursorPaginatedResult<MenuGroup>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string> { "deleted = FALSE" };
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            where.Add("(name ILIKE @Keyword OR icon ILIKE @Keyword)");
            param.Add("Keyword", $"%{request.Keyword}%");
        }

        return await GetListCursorBasedAsync<MenuGroup>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderDirection: request.Ascending ? "ASC" : "DESC",
            idColumn: "id"
        );
    }

    public async Task<List<MenuGroup>> GetTreeAsync()
    {
        // Fetch all active groups in one query, then build tree in memory
        const string sql = """
            SELECT * FROM menu_groups
            WHERE deleted = FALSE AND active = TRUE
            ORDER BY sort ASC
        """;

        var groups = (await _connection.QueryAsync<MenuGroup>(sql)).ToList();

        var lookup = groups.ToDictionary(g => g.Id);
        var roots = new List<MenuGroup>();

        foreach (var g in groups)
        {
            if (g.ParentId.HasValue && lookup.TryGetValue(g.ParentId.Value, out var parent))
            {
                // children list is not on entity – return flat; tree is assembled by caller/DTO
            }
            else
            {
                roots.Add(g);
            }
        }

        return groups; // return flat list; tree assembly happens in controller/DTO mapper
    }
}

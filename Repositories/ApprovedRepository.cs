using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class ApprovedRepository : SimpleCrudRepository<Approved_status, Guid>, IApprovedStatus
{
    private readonly ILogManager _logger;
    public ApprovedRepository(IDbConnection connection, ILogManager logger) : base(connection)
    {
        this._logger = logger;
    }

    public async Task<Approved_status> AddAsync(Approved_status entity)
    {
        try
        {
            entity.id = Uuid7.NewUuid7().ToGuid();
            await CreateAsync(entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to add approved_status", ex);
            throw;
        }
    }

    public async Task DeleteItemAsync(Guid approvedStatusId)
    {
        var entity = new Approved_status { id = approvedStatusId };
        await DeleteAsync(entity);
    }

    public async Task<Approved_status> UpdateItemAsync(Approved_status entity)
    {
        return await UpdateAsync(entity);
    }

    public async Task<CursorPaginatedResult<Approved_status>> GetTaskStatusPageAsync(ApprovedSearch search)
    {
        var where = new List<string>();
        var param = new DynamicParameters();

        // required filter
        if (search.Type is not null)
        {
            where.Add("type = @Type");
            param.Add("Type", search.Type);
        }

        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            where.Add("name ILIKE '%' || @Keyword || '%'");
            param.Add("Keyword", search.Keyword);
        }

        // determine order direction (default desc)
        string orderDirection = search.SortOrderDirection?.ToLower() switch
        {
            "asc" => "ASC",    // if lowercased value is "asc", return "ASC"
            "desc" => "DESC",    // if lowercased value is "desc", return "DESC"
            _ => search.Ascending ? "ASC" : "DESC"    // default: fallback to boolean Ascending
        };

        // Call the generic pagination that supports ordering by sort_order + composite cursor
        return await GetListCursorBasedAsync<Approved_status>(
            request: search,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderByColumn: "id",
            orderDirection: orderDirection
        );
    }

    private async Task<bool> CheckingOrderNumberInType(string type, int sortOrder)
    {
        var sql = """
            SELECT COUNT(1) 
            FROM public.approved_status 
            WHERE type = @Type and sort_order = @SortOrder
        """;
        var checking = await _connection.ExecuteScalarAsync<int>(sql, new { Type = type, SortOrder = sortOrder });
        return checking > 0;
    }

    public async Task<Approved_status> GetByIDAsync(Guid id)
    {
        var result = await base.GetByIDAsync<Guid>(id);
        return result;
    }
}

using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class PopupRepository : SimpleCrudRepository<Popup, Guid>, IPopup
{
    private readonly IAssistantService _assistantService;
    public PopupRepository(IDbConnection connection, IAssistantService assistantService) : base(connection)
    {
        _assistantService = assistantService;
    }

    new public async Task<Popup> GetByIdAsync(Guid id)
    {
        var result = await base.GetByIdAsync(id);
        return result;
    }

    public async Task<CursorPaginatedResult<Popup>> GetPopupPageAsync(PopupSearch request)
    {
        try
        {
            var where = new List<string>();
            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                where.Add("content ILIKE '%' || @Keyword || '%'");
                param.Add("Keyword", request.Keyword);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                where.Add("content ILIKE '%' || @SearchTerm || '%'");
                param.Add("SearchTerm", request.SearchTerm);
            }

            if (request.Type.HasValue)
            {
                where.Add("type = @Type");
                param.Add("Type", request.Type.Value);
            }

            if (request.Validity_start.HasValue)
            {
                where.Add("validity_start >= @Validity_start");
                param.Add("Validity_start", request.Validity_start.Value);
            }

            if (request.Validity_end.HasValue)
            {
                where.Add("validity_end <= @Validity_end");
                param.Add("Validity_end", request.Validity_end.Value);
            }

            if (request.Display_from.HasValue)
            {
                where.Add("display_from >= @Display_from");
                param.Add("Display_from", request.Display_from.Value);
            }

            if (request.Display_to.HasValue)
            {
                where.Add("display_to <= @Display_to");
                param.Add("Display_to", request.Display_to.Value);
            }

            var orderDirection = request.Ascending ? "ASC" : "DESC";

            return await GetListCursorBasedAsync<Popup>(
                request: request,
                extraWhere: string.Join(" AND ", where),
                extraParams: param,
                orderByColumn: "id",
                orderDirection: orderDirection,
                idColumn: "id"
            );
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<Popup> AddAsync(Popup entity)
    {
        if (entity is null)
            throw new BadRequestException("Please provide popup details");
        if (entity.Content is null)
            throw new BadRequestException("Please provide a valid popup content.");
        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistantService.UserId);
        var sql = """
            INSERT INTO popups (id, content, validity_start, validity_end, type, created, updated, display_from, display_to)
            VALUES (@Id, @Content, @Validity_start, @Validity_end, @Type, @Created_at, @Updated_at, @Display_from, @Display_to);
        """;
        try
        {
            var parameters = new
            {
                entity.Id,
                entity.Content,
                entity.Validity_start,
                entity.Validity_end,
                entity.Type,
                Created_at = entity.Created,
                Updated_at = entity.Updated,
                entity.Display_from,
                entity.Display_to
            };

            var inserted = await _connection.ExecuteAsync(sql, parameters);
            if (inserted > 0)
            {
                var result = await this.GetByIdAsync(entity.Id);
                if (result is not null)
                    return result;
                throw new BadRequestException("Popup created, but failed to retrieve");
            }
            throw new BadRequestException("Failed top insert popup into the database");
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
            var existingPopup = await GetByIdAsync(id)
                ?? throw new NotFoundException("Popup not found");

            var result = await this.DeleteByIdAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> UpdateItemAsync(Guid id, Popup entity)
    {
        try
        {
            var existingPopup = await GetByIdAsync(id)
                ?? throw new NotFoundException("Popup not found");

            entity.Id = id;

            entity.Updated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE popups
                SET content = @Content, 
                    validity_start = @Validity_start, 
                    validity_end = @Validity_end,
                    type = @Type,  
                    updated = @Updated, 
                    display_from = @Display_from,
                    display_to = @Display_to,
                WHERE id = @Id;
            """;

            var parameters = new
            {
                entity.Id,
                entity.Content,
                entity.Validity_start,
                entity.Validity_end,
                entity.Type,
                Updated = entity.Updated,
                entity.Display_from,
                entity.Display_to
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update popup.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

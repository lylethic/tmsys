using System;
using System.Data;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ProjectTypeRepository(IDbConnection connection, IAssistantService assistant) : SimpleCrudRepository<ProjectType, Guid>(connection), IProjectTypeRepository
{
    private readonly IAssistantService _assistant = assistant;
    public async Task<ProjectType> AddAsync(ProjectType entity)
    {
        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistant.UserId);
        var sql = """
            INSERT INTO project_types (id, name, description, created, created_by, active)
            VALUES (@Id, @Name, @Description, @Created, @Created_by, @Active);
        """;
        try
        {
            await _connection.ExecuteAsync(sql, entity);
            var result = await GetByIdAsync(entity.Id);
            if (result is not null)
                return result;
            throw new BadRequestException("Failed to add item.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await SoftDeleteAsync(id);
        return true;
    }

    public async Task<CursorPaginatedResult<ProjectTypeModel>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();
        where.Add("deleted = false");
        if (request.Keyword is not null)
        {
            where.Add("(name ILIKE @Keyword OR description ILIKE @Keyword)");
            param.Add("Keyword", $"%{request.Keyword}%");
        }
        var page = await this.GetListCursorBasedAsync<ProjectTypeModel>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            idColumn: "id"
        );
        var result = new CursorPaginatedResult<ProjectTypeModel>
        {
            NextCursor = page.NextCursor,
            Data = page.Data,
            HasNextPage = page.HasNextPage,
            Total = page.Total
        };
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, ProjectType entity)
    {
        entity.Id = id;
        entity.Updated_by = Guid.Parse(_assistant.UserId);
        var sql = """
            UPDATE project_types SET
                name=@Name,
                description=@Description,
                updated=@Updated,
                updated_by=@Updated_by,
                active=@Active
            WHERE id=@Id;
        """;
        await _connection.ExecuteAsync(sql, entity);
        return true;
    }
}

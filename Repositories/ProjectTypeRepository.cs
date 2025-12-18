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
        entity.id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistant.UserId);
        var sql = """
            INSERT INTO project_types (id, name, description, created, created_by, active)
            VALUES (@Id, @Name, @Description, @Created, @Created_by, @Active);
        """;
        try
        {
            await _connection.ExecuteAsync(sql, entity);
            var result = await GetByIdAsync(entity.id);
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

    public async Task<PaginatedResult<ProjectTypeModel>> GetAllAsync(BaseSearch request)
    {
        var sql = """
            SELECT * FROM public.get_list_of_project_types (
                @page_index,
                @page_size,
                @search_term,
                @order_by
            );
        """;
        return await GetListWithPaginationAndFilters<BaseSearch, ProjectTypeModel>(
            filter: request,
            sqlQuery: sql,
            parameterMapper: filter => new
            {
                search_term = filter?.SearchTerm,
                page_index = filter != null ? filter.PageIndex : 1,
                page_size = filter != null ? filter.PageSize : 20,
                order_by = filter != null ? filter.OrderBy : 1
            });
    }

    public async Task<PaginatedResult<ProjectType>> GetAllAsync(PaginationRequest request)
    {
        var result = await GetListWithPagination(request);
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, ProjectType entity)
    {
        entity.id = id;
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

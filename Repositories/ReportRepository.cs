using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ReportRepository : SimpleCrudRepository<Report, Guid>, IReportRepository
{
    private readonly IProjectRepository _projectrepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssistantService _assistantService;

    public ReportRepository(IDbConnection connection, IUserRepository userRepository, IAssistantService assistantService, IProjectRepository projectrepository)
    : base(connection)
    {
        _userRepository = userRepository;
        _assistantService = assistantService;
        _projectrepository = projectrepository;
    }

    public async Task<Report> AddAsync(Report entity)
    {
        if (entity is null)
            throw new BadRequestException("Please provide report details.");

        if (entity.Content is null)
        {
            throw new BadRequestException("Please provide a valid report content.");
        }
        if (entity.Type is null)
        {
            throw new BadRequestException("Please provide a valid report type.");
        }

        entity.id = Uuid7.NewUuid7().ToGuid();

        var existingProjectId = await _projectrepository.GetByIdAsync(entity.Project_id);
        if (existingProjectId == null)
            throw new NotFoundException($"Project with ID '{entity.Project_id}' does not exist.");

        entity.Created_by = Guid.Parse(_assistantService.UserId);
        entity.Generated_by = Guid.Parse(_assistantService.UserId);

        var sql = """
            INSERT INTO reports (
                id, project_id, report_date, content, type, generated_by,
                created, updated, created_by, updated_by, deleted, active
            ) 
            VALUES (
                @Id, @Project_id, @Report_date, @Content, @Type, @Generated_by,
                @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active
            )
        """;

        try
        {
            var queryResult = await _connection.ExecuteAsync(sql, entity);

            if (queryResult > 0)
            {
                var result = await GetByIdAsync(entity.id);
                if (result != null)
                    return result;
                throw new BadRequestException("Report created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert report into the database.");
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
            var existingReport = await GetByIdAsync(id)
                ?? throw new NotFoundException("Report not found");

            var sql = """
                UPDATE reports 
                SET deleted = true, active = false, updated = @Updated, updated_by = @Updated_by 
                WHERE id = @Id AND deleted = false
            """;

            var parameters = new
            {
                Id = id,
                Updated_by = Guid.Parse(_assistantService.UserId)
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete report.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<Report>> GetAllAsync(PaginationRequest request)
    {
        var sql = """
            SELECT * FROM reports
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM reports WHERE deleted = false;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Report>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Report>
            {
                Data = result.Count > 0 ? result : new List<Report>(),
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

    public async Task<PaginatedResult<ReportModel>> GetAllAsync(ReportSearch request)
    {
        var sql = """
            SELECT * FROM get_list_of_reports(
                @page_index, 
                @page_size, 
                @searchTerm,
                @p_report_date,
                @p_project_id,
                @p_type
            );
        """;
        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };
        return await GetListWithPaginationAndFilters<ReportSearch, ReportModel>(
            filter: request,
            sqlQuery: sql,
            parameterMapper: filter => new
            {
                searchTerm = filter?.SearchTerm,
                page_index = filter != null ? filter.PageIndex : 1,
                page_size = filter != null ? filter.PageSize : 20,
                p_report_date = filter?.P_report_date,
                p_project_id = filter?.Project_id,
                p_type = filter?.P_type
            });
    }

    public async Task<Report> GetByIdAsync(Guid id)
    {
        var result = await base.GetByIdAsync(id);
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, Report entity)
    {
        try
        {
            var existingReport = await GetByIdAsync(id)
                ?? throw new NotFoundException("Report not found");

            entity.id = id;
            var existingProjectId = await _projectrepository.GetByIdAsync(entity.Project_id);
            if (existingProjectId == null)
                throw new NotFoundException($"Project with ID '{entity.Project_id}' does not exist.");

            entity.Updated_by = Guid.Parse(_assistantService.UserId);
            entity.Generated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE reports
                SET project_id = @Project_id, 
                    report_date = @Report_date, 
                    content = @Content, 
                    type = @Type, 
                    generated_by = @Generated_by, 
                    updated = @Updated, 
                    updated_by = @Updated_by
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update report.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

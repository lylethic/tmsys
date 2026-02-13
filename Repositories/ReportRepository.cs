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

    public async Task<CursorPaginatedResult<Report>> GetAllAsync(ReportSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();

        where.Add("deleted = false");

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            where.Add("(content ILIKE '%' || @Keyword || '%' OR type ILIKE '%' || @Keyword || '%')");
            param.Add("Keyword", request.Keyword);
        }

        if (request.P_report_date.HasValue)
        {
            where.Add("DATE(report_date) = DATE(@ReportDate)");
            param.Add("ReportDate", request.P_report_date.Value);
        }

        if (request.Project_id.HasValue && request.Project_id.Value != Guid.Empty)
        {
            where.Add("project_id = @ProjectId");
            param.Add("ProjectId", request.Project_id.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.P_type))
        {
            where.Add("type ILIKE @Type");
            param.Add("Type", request.P_type);
        }

        return await this.GetListCursorBasedAsync<Report>(
           request: request,
           extraWhere: string.Join(" AND ", where),
           extraParams: param,
           orderDirection: request.Ascending ? "ASC" : "DESC",
           idColumn: "id"
        );
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

        entity.Id = Uuid7.NewUuid7().ToGuid();

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
                var result = await GetByIdAsync(entity.Id);
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

    public async Task<bool> UpdateItemAsync(Guid id, Report entity)
    {
        try
        {
            var existingReport = await GetByIdAsync(id)
                ?? throw new NotFoundException("Report not found");

            entity.Id = id;
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

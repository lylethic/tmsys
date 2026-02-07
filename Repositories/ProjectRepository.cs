using System;
using System.Data;
using System.Text.Json;
using AutoMapper;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.CoreConstans;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ProjectRepository : SimpleCrudRepository<Project, Guid>, IProjectRepository
{
    private readonly IAssistantService _assistantService;
    private readonly IUserRepository _userRepository;
    private readonly Lazy<IProjectMember> _projectMemberRepository;
    private readonly IMapper _mapper;

    public ProjectRepository(IDbConnection connection,
        IAssistantService assistantService,
        IUserRepository userRepository,
        Lazy<IProjectMember> projectMemberRepository,
        IMapper mapper)
    : base(connection)
    {
        _assistantService = assistantService;
        _userRepository = userRepository;
        _projectMemberRepository = projectMemberRepository;
        _mapper = mapper;
    }

    public async Task<Project> AddAsync(Project entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide project details.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        var existingManagerId = await _userRepository.GetByIdAsync(entity.Manager_id);
        if (existingManagerId == null)
            throw new BadRequestException("Invalid manager ID.");

        entity.Created_by = Guid.Parse(_assistantService.UserId);
        if (entity.Status is null)
        {
            entity.Status = CoreConstants.ProjectStatus.Pending.ToString();
        }
        var sql = """
            INSERT INTO projects (
                id, name, description, start_date, end_date, manager_id, status, 
                created, updated, created_by, updated_by, deleted, active, project_type
            ) 
            VALUES (
                @Id, @Name, @Description, @Start_date, @End_date, @Manager_id, @Status, 
                @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active, @Project_type
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
                throw new BadRequestException("Project created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert project into the database.");
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
            var existingProject = await GetByIdAsync(id)
                ?? throw new NotFoundException("Project not found");
            var sql = """
                UPDATE projects 
                SET deleted = true, active = false, updated = @Updated, updated_by = @Updated_by 
                WHERE id = @Id AND deleted = false
            """;

            var parameters = new
            {
                Id = id,
                Updated = DateTime.UtcNow,
                Updated_by = Guid.Parse(_assistantService.UserId)
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete project.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<Project>> GetAllAsync(PaginationRequest request)
    {
        var sql = """
            SELECT * FROM projects
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM projects WHERE deleted = false;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Project>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Project>
            {
                Data = result.Count > 0 ? result : new List<Project>(),
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

    public async Task<CursorPaginatedResult<ProjectModel>> GetAllAsync(ProjectSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();

        where.Add("deleted = false");

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            where.Add("name ILIKE @Name");
            param.Add("Name", $"%{request.Keyword}%");
        }

        if (request.ManagerId != null)
        {
            where.Add("manager_id = @ManagerId");
            param.Add("ManagerId", request.ManagerId);
        }
        else
        {
            where.Add("manager_id = @ManagerId");
            param.Add("ManagerId", Guid.Parse(_assistantService.UserId));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            where.Add("status = @Status");
            param.Add("Status", request.Status);
        }

        if (request.StartDate.HasValue)
        {
            where.Add("start_date >= @StartDate");
            param.Add("StartDate", request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            where.Add("end_date <= @EndDate");
            param.Add("EndDate", request.EndDate);
        }

        string orderDirection = request.SortOrderDirection?.ToLower() switch
        {
            "asc" => "ASC",    // if lowercased value is "asc", return "ASC"
            "desc" => "DESC",    // if lowercased value is "desc", return "DESC"
            _ => request.Ascending ? "ASC" : "DESC"    // default: fallback to boolean Ascending
        };

        var page = await this.GetListCursorBasedAsync<ProjectModel>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderByColumn: "id",
            orderDirection: orderDirection,
            idColumn: "id"
        );

        var result = new CursorPaginatedResult<ProjectModel>
        {
            NextCursor = page.NextCursor,
            Data = page.Data,
            HasNextPage = page.HasNextPage,
            Total = page.Total
        };

        return result;
    }
    public async Task<ProjectExtendedModel> GetExtendProjectByIdAsync(Guid id)
    {
        var sql = """
            SELECT
                p.id,
                p.name,
                p.description,
                p.start_date,
                p.end_date,
                p.status,
                COALESCE(
                    jsonb_agg(
                        jsonb_build_object(
                            'member_id', pm.member_id,
                            'name', u.name,
                            'profilepic', u.profilepic
                        )
                    ) FILTER (WHERE u.id IS NOT NULL),
                    '[]'::jsonb
                )::text AS extend_users_json,
                (
                    SELECT jsonb_build_object('name', pt.name, 'description', pt.description)::text
                    FROM public.project_types pt
                    WHERE pt.id = p.project_type
                    AND pt.deleted = false
                    AND pt.active = true
                ) AS extend_project_type_json
            FROM
                public.projects AS p
            LEFT JOIN public.project_members AS pm
                ON p.id = pm.project_id
                AND pm.deleted = false
                AND pm.active = true
            LEFT JOIN public.users AS u
                ON pm.member_id = u.id
                AND u.deleted = false
                AND u.active = true
            WHERE
                p.id = @id::uuid
            GROUP BY
                p.id;
        """;

        var rawResult = await _connection.QuerySingleOrDefaultAsync<ProjectRawDto>(sql, new { Id = id })
            ?? throw new NotFoundException("Project not found");

        return _mapper.Map<ProjectExtendedModel>(rawResult);
    }

    public async Task<bool> UpdateItemAsync(Guid id, Project entity)
    {
        try
        {
            var existingProject = await GetByIdAsync(id)
                ?? throw new NotFoundException("Project not found");

            entity.Id = id;
            var existingManagerId = await _userRepository.GetByIdAsync(entity.Manager_id);
            if (existingManagerId == null)
                throw new BadRequestException("Invalid manager ID.");
            entity.Updated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE projects
                SET name = @Name, 
                    description = @Description, 
                    start_date = @Start_date, 
                    end_date = @End_date, 
                    manager_id = @Manager_id, 
                    status = @Status, 
                    updated = @Updated, 
                    updated_by = @Updated_by,
                    project_type = @Project_type
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update project.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ProjectMemberRepository : SimpleCrudRepository<ProjectMember, Guid>, IProjectMember
{
    private readonly IAssistantService _assistantService;
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;
    public ProjectMemberRepository(IDbConnection connection,
        IProjectRepository projectRepo,
        IUserRepository userRepo,
        IAssistantService assistantService) : base(connection)
    {
        this._projectRepo = projectRepo;
        this._userRepo = userRepo;
        this._assistantService = assistantService;
    }
    public async Task<ProjectMemberModel> GetProjectMemberAsync(Guid id)
    {
        var sql = """
            SELECT * FROM project_members 
            WHERE id = @Id AND deleted = false
        """;

        var page = await _connection.QuerySingleOrDefaultAsync<ProjectMember>(sql, new { Id = id })
            ?? throw new NotFoundException("Not found");

        var extendProject = await _projectRepo.GetByIdAsync(page.project_id);
        var extendUser = await _userRepo.GetByIdAsync(page.member_id);

        var result = MapToProjectMemberModel(page, extendProject, extendUser);
        return result;
    }

    public async Task<CursorPaginatedResult<ProjectMemberModel>> GetProjectMemberPageAsync(ProjectMemberSearch search)
    {
        var where = new List<string>();
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            where.Add("role ILIKE '%' || @Keyword || '%'");
            param.Add("Keyword", search.Keyword);
        }

        if (search.ProjectId.HasValue && search.ProjectId.Value != Guid.Empty)
        {
            where.Add("project_id=@ProjectId");
            param.Add("ProjectId", search.ProjectId.Value);
        }

        if (search.MemberId.HasValue && search.MemberId.Value != Guid.Empty)
        {
            where.Add("member_id=@MemberId");
            param.Add("MemberId", search.MemberId.Value);
        }

        if (search.LeftAt.HasValue)
        {
            where.Add("DATE(left_at) = DATE(@LeftAt)");
            param.Add("LeftAt", search.LeftAt.Value);
        }
        string orderDirection = search.SortOrderDirection?.ToLower() switch
        {
            "asc" => "ASC",    // if lowercased value is "asc", return "ASC"
            "desc" => "DESC",    // if lowercased value is "desc", return "DESC"
            _ => search.Ascending ? "ASC" : "DESC"    // default: fallback to boolean Ascending
        };

        var page = await GetListByIdCursorNoDeleleColAsync<ProjectMemberModel>(
            request: search,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderByColumn: "id",
            orderDirection: orderDirection,
            idColumn: "id"
        );

        var result = new CursorPaginatedResult<ProjectMemberModel>
        {
            NextCursor = page.NextCursor,
            NextCursorSortOrder = page.NextCursorSortOrder,
            HasNextPage = page.HasNextPage,
            Total = page.Total
        };

        var projectIds = page.Data
            .Select(x => x.project_id)
            .Distinct()
            .ToArray();

        var memberIds = page.Data
            .Select(x => x.member_id)
            .Distinct()
            .ToArray();

        var projects = await _connection.QueryAsync<Project>(
            "SELECT * FROM projects WHERE id = ANY(@Ids)",
            new { Ids = projectIds });

        var users = await _connection.QueryAsync<User>(
            "SELECT * FROM users WHERE id = ANY(@Ids)",
            new { Ids = memberIds });

        var projectDict = projects.ToDictionary(x => x.Id);
        var userDict = users.ToDictionary(x => x.Id);

        foreach (var item in page.Data)
        {
            projectDict.TryGetValue(item.project_id, out var project);
            userDict.TryGetValue(item.member_id, out var user);

            result.Data.Add(
                MapToProjectMemberModel(item, project, user)
            );
        }

        return result;
    }

    public async Task<ProjectMember> AddAsync(ProjectMember entity)
    {
        entity.id = Uuid7.NewUuid7().ToGuid();
        entity.created_by = Guid.Parse(_assistantService.UserId);
        await CreateAsync(entity);
        return entity;
    }

    /// <summary>
    /// Remove Member
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteItemAsync(Guid id)
    {
        var now = DateTime.UtcNow;
        var sql = """
            UPDATE public.project_members 
            SET left_at = @now, active = false
            WHERE id = @id;
        """;
        var result = await _connection.ExecuteAsync(sql, new { left_at = now });
        return result > 0;
    }

    public async Task<bool> UpdateItemAsync(Guid id, ProjectMember entity)
    {
        entity.id = id;
        var sql = """
            UPDATE public.project_members 
            SET project_id = @project_id,
                member_id = @member_id,
                role = @role,
                left_at = @left_at,
                active = @active
            WHERE id = @id;
        """;
        var result = await _connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    private static ProjectMemberModel MapToProjectMemberModel(ProjectMember projectMember, Project extendProject, User extendUser)
    {
        return new ProjectMemberModel
        {
            id = projectMember.id,
            project_id = projectMember.project_id,
            member_id = projectMember.member_id,
            role = projectMember.role,
            left_at = projectMember.left_at,
            created = projectMember.created,
            updated = projectMember.updated,
            created_by = projectMember.created_by,
            updated_by = projectMember.updated_by,
            deleted = projectMember.deleted,
            active = projectMember.active,
            extend_project = new
            {
                name = extendProject.Name,
                description = extendProject.Description,
                start_date = extendProject.Start_date,
                end_date = extendProject.End_date,
                status = extendProject.Status,
                project_type = extendProject.Project_type
            },
            extend_user = new
            {
                name = extendUser.Name,
                profilepic = extendUser.ProfilePic
            }
        };
    }
}

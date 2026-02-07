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
using System.Data;

namespace server.Repositories;

public class WorkScheduleRepository : SimpleCrudRepository<Work_schedule, Guid>, IWorkSchedule
{
    private readonly IAssistantService _assistantService;
    public WorkScheduleRepository(IDbConnection connection, IAssistantService assistantService) : base(connection)
    {
        _assistantService = assistantService;
    }

    public async Task<CursorPaginatedResult<Work_schedule>> GetAllAsync(WorkScheduleSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(request.InternEmail))
        {
            where.Add("intern_email ILIKE '%' || @InternEmail || '%'");
            param.Add("InternEmail", request.InternEmail);
        }
        if (!string.IsNullOrWhiteSpace(request.MentorEmail))
        {
            where.Add("mentor_email ILIKE '%' || @MentorEmail || '%'");
            param.Add("MentorEmail", request.MentorEmail);
        }
        if (request.WeekStart.HasValue)
        {
            where.Add("week_start >= @WeekStart");
            param.Add("WeekStart", request.WeekStart.Value.ToUniversalTime());
        }
        if (request.WeekEnd.HasValue)
        {
            where.Add("week_end <= @WeekEnd");
            param.Add("WeekEnd", request.WeekEnd.Value.ToUniversalTime());
        }

        var orderDirection = request.Ascending ? "ASC" : "DESC";

        return await this.GetListCursorBasedAsync<Work_schedule>(
           request: request,
           extraWhere: string.Join(" AND ", where),
           extraParams: param,
           orderByColumn: "id",
           orderDirection: orderDirection,
           idColumn: "id"
         );
    }

    public async Task<CursorPaginatedResult<MenteeDto>> GetMenteesByMentorEmailAsync(string mentorEmail, CursorPaginationRequest request, DateTimeOffset? weekStart, DateTimeOffset? weekEnd)
    {
        if (string.IsNullOrWhiteSpace(mentorEmail))
            throw new BadRequestException("Mentor email is required.");

        weekStart = weekStart?.ToUniversalTime();
        weekEnd = weekEnd?.ToUniversalTime().AddDays(1).AddTicks(-1);

        const string sqlMentor = """
            select u.id
            from users u
            join user_roles ur on u.id = ur.user_id
            join roles r on ur.role_id = r.id
            where lower(u.email) = lower(@MentorEmail)
              -- and r.name = 'Mentor'
              and u.active = true
              and u.deleted = false
            limit 1;
        """;

        var mentorId = await _connection.ExecuteScalarAsync<Guid?>(sqlMentor, new { MentorEmail = mentorEmail });
        if (!mentorId.HasValue)
            throw new NotFoundException("Mentor not found or role is not Mentor.");

        var orderDirection = request.Ascending ? "ASC" : "DESC";
        var pageSize = request.PageSize;
        var cursorCondition = string.Empty;

        if (request.Cursor.HasValue)
        {
            cursorCondition = orderDirection == "ASC"
                ? "AND ws.id > @Cursor"
                : "AND ws.id < @Cursor";
        }

        var sqlMentees = $"""
            select distinct
                u.id as UserId,
                coalesce(u.email, ws.intern_email) as Email,
                coalesce(u.name, ws.full_name) as Name,
                ws.monday,
                ws.tuesday,
                ws.wednesday,
                ws.thursday,
                ws.friday,
                TO_CHAR(ws.week_start, 'YYYY-MM-DD') as Week_start,
                TO_CHAR(ws.week_end, 'YYYY-MM-DD') as Week_end,
                ws.id as Id
            from work_schedule ws
            left join users u on ws.intern_id = u.id
            where lower(ws.mentor_email) = lower(@MentorEmail)
              and (@WeekStart is null or ws.week_end >= @WeekStart)
              and (@WeekEnd is null or ws.week_start <= @WeekEnd)
              and ws.deleted = false
              and ws.active = true
              {cursorCondition}
            order by ws.id {orderDirection}
            limit @PageSize;
            """;

        var mentees = await _connection.QueryAsync<MenteeDto>(sqlMentees, new
        {
            MentorEmail = mentorEmail,
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            Cursor = request.Cursor,
            PageSize = pageSize + 1
        });

        var menteesList = mentees.ToList();
        var hasNextPage = menteesList.Count > pageSize;

        if (hasNextPage)
            menteesList = [.. menteesList.Take(pageSize)];

        var nextCursor = hasNextPage ? menteesList.LastOrDefault()?.Id : null;

        return new CursorPaginatedResult<MenteeDto>
        {
            Data = menteesList,
            NextCursor = nextCursor,
            HasNextPage = hasNextPage
        };
    }

    public async Task<Work_schedule> GetByIdAsync(Guid id)
    {
        var result = await base.GetByIdAsync(id);
        return result;
    }

    public async Task<Work_schedule> AddAsync(Work_schedule entity)
    {
        if (entity is null)
            throw new BadRequestException("Please provide work schedule details.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistantService.UserId);
        entity.Week_start = entity.Week_start?.ToUniversalTime();
        entity.Week_end = entity.Week_end?.ToUniversalTime();

        var sql = """
            INSERT INTO work_schedule (
                id, intern_id, intern_email, mentor_email, week_start, week_end,
                monday, tuesday, wednesday, thursday, friday,
                full_name, created, updated, created_by, updated_by, deleted, active
            )
            VALUES (
                @Id, @Intern_id, @Intern_email, @Mentor_email, @Week_start, @Week_end,
                @Monday, @Tuesday, @Wednesday, @Thursday, @Friday,
                @Full_name, @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active
            );
        """;

        try
        {
            var inserted = await _connection.ExecuteAsync(sql, entity);
            if (inserted > 0)
            {
                var result = await GetByIdAsync(entity.Id);
                if (result is not null)
                    return result;
                throw new BadRequestException("Work schedule created, but failed to retrieve.");
            }
            throw new BadRequestException("Failed to insert work schedule into the database.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> UpdateItemAsync(Guid id, Work_schedule entity)
    {
        try
        {
            var existing = await GetByIdAsync(id)
                ?? throw new NotFoundException("Work schedule not found");

            entity.Id = id;
            entity.Updated_by = Guid.Parse(_assistantService.UserId);
            entity.Week_start = entity.Week_start?.ToUniversalTime();
            entity.Week_end = entity.Week_end?.ToUniversalTime();

            var sql = """
                UPDATE work_schedule
                SET
                    intern_id = @Intern_id,
                    intern_email = @Intern_email,
                    mentor_email = @Mentor_email,
                    week_start = @Week_start,
                    week_end = @Week_end,
                    monday = @Monday,
                    tuesday = @Tuesday,
                    wednesday = @Wednesday,
                    thursday = @Thursday,
                    friday = @Friday,
                    full_name = @Full_name,
                    updated = @Updated,
                    updated_by = @Updated_by,
                    active = @Active
                WHERE id = @Id;
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update work schedule.");
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
            _ = await GetByIdAsync(id)
                ?? throw new NotFoundException("Work schedule not found");

            await SoftDeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}

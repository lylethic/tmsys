using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class WorkScheduleRepository : SimpleCrudRepository<Work_schedule, Guid>, IWorkSchedule
{
    private readonly IAssistantService _assistantService;
    public WorkScheduleRepository(IDbConnection connection, IAssistantService assistantService) : base(connection)
    {
        _assistantService = assistantService;
    }

    public async Task<PaginatedResult<Work_schedule>> GetAllAsync(PaginationRequest request)
    {
        return await GetListWithPagination(request);
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

        var sql = """
            INSERT INTO work_schedule (
                id, intern_id, intern_email, mentor_email,
                monday, tuesday, wednesday, thursday, friday,
                full_name, created, updated, created_by, updated_by, deleted, active
            )
            VALUES (
                @Id, @Intern_id, @Intern_email, @Mentor_email,
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

            var sql = """
                UPDATE work_schedule
                SET
                    intern_id = @Intern_id,
                    intern_email = @Intern_email,
                    mentor_email = @Mentor_email,
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

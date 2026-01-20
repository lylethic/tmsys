using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class CompanyGeofenceRepository : SimpleCrudRepository<CompanyGeofence, Guid>, ICompanyGeofence
{
    private readonly IAssistantService _assistantService;

    public CompanyGeofenceRepository(IDbConnection connection, IAssistantService assistantService) : base(connection)
    {
        _assistantService = assistantService;
    }

    public async Task<CompanyGeofence?> GetActiveAsync()
    {
        const string sql = """
            select *
            from company_geofences
            where active = true and deleted = false
            order by created desc
            limit 1;
        """;

        return await _connection.QueryFirstOrDefaultAsync<CompanyGeofence>(sql);
    }

    public async Task<CompanyGeofence> AddAsync(CompanyGeofence entity)
    {
        if (entity is null)
            throw new BadRequestException("Please provide company geofence details.");

        Validate(entity);

        var now = DateTime.UtcNow;
        var userId = Guid.Parse(_assistantService.UserId);

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created = now;
        entity.Updated = now;
        entity.Created_by = userId;
        entity.Updated_by = userId;
        entity.Deleted = false;
        entity.Active ??= true;

        const string sqlInsert = """
            insert into company_geofences
            (id, name, center_lat, center_lng, radius_m, active, deleted, created, updated, created_by, updated_by)
            values
            (@Id, @Name, @Center_lat, @Center_lng, @Radius_m, @Active, @Deleted, @Created, @Updated, @Created_by, @Updated_by);
        """;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using var transaction = _connection.BeginTransaction();
        try
        {
            if (entity.Active == true)
            {
                const string sqlDeactivate = """
                    update company_geofences
                    set active = false,
                        updated = @Updated,
                        updated_by = @Updated_by
                    where active = true and deleted = false;
                """;
                await _connection.ExecuteAsync(sqlDeactivate, new { Updated = now, Updated_by = userId }, transaction);
            }

            var inserted = await _connection.ExecuteAsync(sqlInsert, entity, transaction);
            if (inserted <= 0)
                throw new BadRequestException("Failed to insert company geofence into the database.");

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new InternalErrorException(ex.Message);
        }

        return await GetByIdAsync(entity.Id)
            ?? throw new BadRequestException("Company geofence created, but failed to retrieve.");
    }

    public async Task<bool> UpdateItemAsync(Guid id, CompanyGeofence entity)
    {
        if (entity is null)
            throw new BadRequestException("Please provide company geofence details.");

        Validate(entity);

        var existing = await GetByIdAsync(id)
            ?? throw new NotFoundException("Company geofence not found.");

        var now = DateTime.UtcNow;
        var userId = Guid.Parse(_assistantService.UserId);

        entity.Id = id;
        entity.Updated = now;
        entity.Updated_by = userId;
        entity.Active ??= existing.Active;

        const string sqlUpdate = """
            update company_geofences
            set name = @Name,
                center_lat = @Center_lat,
                center_lng = @Center_lng,
                radius_m = @Radius_m,
                active = @Active,
                updated = @Updated,
                updated_by = @Updated_by
            where id = @Id;
        """;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using var transaction = _connection.BeginTransaction();
        try
        {
            if (entity.Active == true)
            {
                const string sqlDeactivate = """
                    update company_geofences
                    set active = false,
                        updated = @Updated,
                        updated_by = @Updated_by
                    where id <> @Id and active = true and deleted = false;
                """;
                await _connection.ExecuteAsync(sqlDeactivate, new { entity.Id, Updated = now, Updated_by = userId }, transaction);
            }

            var updated = await _connection.ExecuteAsync(sqlUpdate, entity, transaction);
            if (updated <= 0)
                throw new BadRequestException("Failed to update company geofence.");

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        _ = await GetByIdAsync(id)
            ?? throw new NotFoundException("Company geofence not found.");

        var now = DateTime.UtcNow;
        var userId = Guid.Parse(_assistantService.UserId);

        const string sqlDelete = """
            update company_geofences
            set deleted = true,
                active = false,
                updated = @Updated,
                updated_by = @Updated_by
            where id = @Id;
        """;

        var affected = await _connection.ExecuteAsync(sqlDelete, new { Id = id, Updated = now, Updated_by = userId });
        return affected > 0;
    }

    private static void Validate(CompanyGeofence entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
            throw new BadRequestException("Company geofence name is required.");

        if (entity.Center_lat is < -90 or > 90)
            throw new BadRequestException("Invalid center latitude.");

        if (entity.Center_lng is < -180 or > 180)
            throw new BadRequestException("Invalid center longitude.");

        if (entity.Radius_m <= 0)
            throw new BadRequestException("Radius must be greater than 0.");
    }
}

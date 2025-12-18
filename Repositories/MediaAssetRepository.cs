using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<MediaAssetRepository> _logger;
    private readonly IAssistantService _assistantService;

    public MediaAssetRepository(
        IDbConnection dbConnection,
        ILogger<MediaAssetRepository> logger,
        IAssistantService assistantService)
    {
        _dbConnection = dbConnection;
        _logger = logger;
        this._assistantService = assistantService;
    }

    public async Task<MediaAsset> CreateAsync(MediaAsset mediaAsset)
    {
        mediaAsset.id = Uuid7.NewUuid7().ToGuid();
        mediaAsset.user_id = Guid.Parse(_assistantService.UserId);
        mediaAsset.created_by = Guid.Parse(_assistantService.UserId);
        const string sql = """
                INSERT INTO media_assets (
                    id, public_id, url, secure_url, resource_type, format, bytes, 
                    width, height, user_id, project_id, task_id, entity_type, 
                    entity_id, description, created, created_by, deleted, active
                ) VALUES (
                    @id, @public_id, @url, @secure_url, @resource_type, @format, @bytes,
                    @width, @height, @user_id, @project_id, @task_id, @entity_type,
                    @entity_id, @description, @created, @created_by, @deleted, @active
                )
                RETURNING *
            """;

        return await _dbConnection.QuerySingleAsync<MediaAsset>(sql, mediaAsset);
    }

    public async Task<List<MediaAsset>> CreateMultipleAsync(List<MediaAsset> mediaAssets)
    {
        foreach (var asset in mediaAssets)
        {
            asset.user_id = Guid.Parse(_assistantService.UserId);
            asset.created_by = Guid.Parse(_assistantService.UserId);
        }
        const string sql = """
                INSERT INTO media_assets (
                    id, public_id, url, secure_url, resource_type, format, bytes, 
                    width, height, user_id, project_id, task_id, entity_type, 
                    entity_id, description, created, created_by, deleted, active
                ) VALUES (
                     @id, @public_id, @url, @secure_url, @resource_type, @format, @bytes,
                    @width, @height, @user_id, @project_id, @task_id, @entity_type,
                    @entity_id, @description, @created, @created_by, @deleted, @active
                )
                RETURNING *
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql, mediaAssets);
        return results.ToList();
    }

    public async Task<MediaAsset> GetByIdAsync(Guid id)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE id = @Id AND deleted = FALSE
            """;

        return await _dbConnection.QuerySingleOrDefaultAsync<MediaAsset>(sql, new { Id = id }) ?? new MediaAsset();
    }

    public async Task<MediaAsset> GetByPublicIdAsync(string publicId)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE public_id = @PublicId AND deleted = FALSE
            """;

        return await _dbConnection.QuerySingleOrDefaultAsync<MediaAsset>(sql, new { PublicId = publicId }) ?? new MediaAsset();
    }

    public async Task<List<MediaAsset>> GetByEntityAsync(string entityType, Guid entityId)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE entity_type = @EntityType 
                AND entity_id = @EntityId 
                AND deleted = FALSE AND active = TRUE
                ORDER BY created DESC
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql, new { EntityType = entityType, EntityId = entityId });
        return results.ToList();
    }

    public async Task<List<MediaAsset>> GetByUserIdAsync(Guid userId)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE user_id = @UserId AND deleted = FALSE AND active = TRUE
                ORDER BY created DESC
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql, new { UserId = userId });
        return results.ToList();
    }

    public async Task<List<MediaAsset>> GetByProjectIdAsync(Guid projectId)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE project_id = @ProjectId AND deleted = FALSE AND active = TRUE
                ORDER BY created DESC
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql, new { ProjectId = projectId });
        return results.ToList();
    }

    public async Task<List<MediaAsset>> GetByTaskIdAsync(Guid taskId)
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE task_id = @TaskId AND deleted = FALSE AND active = TRUE
                ORDER BY created DESC
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql, new { TaskId = taskId });
        return results.ToList();
    }

    public async Task<bool> UpdateAsync(MediaAsset mediaAsset)
    {
        mediaAsset.updated = DateTime.UtcNow;
        mediaAsset.updated_by = Guid.Parse(_assistantService.UserId);
        const string sql = """
                UPDATE media_assets 
                SET description = @Description,
                    entity_type = @EntityType,
                    entity_id = @EntityId,
                    updated = @Updated,
                    updated_by = @UpdatedBy,
                    active = @Active
                WHERE id = @Id AND deleted = FALSE
            """;

        var affected = await _dbConnection.ExecuteAsync(sql, mediaAsset);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var existingMediaAssest = await GetByIdAsync(id);
        if (existingMediaAssest is null)
        {
            throw new NotFoundException("Media asset not found");
        }

        const string sql = """
                UPDATE media_assets 
                SET deleted = TRUE, 
                    active = FALSE,
                    updated = @Updated,
                    updated_by = @DeletedBy
                WHERE id = @Id
            """;

        var affected = await _dbConnection.ExecuteAsync(sql, new
        {
            Id = id,
            Updated = DateTime.UtcNow,
            DeletedBy = Guid.Parse(_assistantService.UserId)
        });
        return affected > 0;
    }

    public async Task<bool> SoftDeleteByPublicIdAsync(string publicId)
    {
        const string sql = """
                UPDATE media_assets 
                SET deleted = TRUE, 
                    active = FALSE,
                    updated = @Updated,
                    updated_by = @DeletedBy
                WHERE public_id = @PublicId
            """;

        var affected = await _dbConnection.ExecuteAsync(sql, new
        {
            PublicId = publicId,
            Updated = DateTime.UtcNow,
            DeletedBy = Guid.Parse(_assistantService.UserId)
        });
        return affected > 0;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM media_assets WHERE id = @Id";
        var affected = await _dbConnection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<int> GetTotalSizeByUserAsync(Guid userId)
    {
        const string sql = """
                SELECT COALESCE(SUM(bytes), 0)::int 
                FROM media_assets 
                WHERE user_id = @userId AND deleted = FALSE
            """;

        return await _dbConnection.ExecuteScalarAsync<int>(sql, new { userId });
    }

    public async Task<List<MediaAsset>> GetAllActiveAsync()
    {
        const string sql = """
                SELECT * FROM media_assets 
                WHERE deleted = FALSE AND active = TRUE
                ORDER BY created DESC
            """;

        var results = await _dbConnection.QueryAsync<MediaAsset>(sql);
        return results.ToList();
    }
}

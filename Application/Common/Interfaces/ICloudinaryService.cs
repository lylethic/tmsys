using System;
using server.Application.Enums;
using server.Common.Models;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload single image to Cloudinary and save to database
    /// </summary>
    Task<CloudinaryUploadResult> UploadImageAsync(
        IFormFile file,
        ImageUploadQuality quality,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null,
        string description = null);

    /// <summary>
    /// Upload multiple images to Cloudinary and save to database
    /// </summary>
    Task<List<CloudinaryUploadResult>> UploadMultipleImagesAsync(
        List<IFormFile> files,
        ImageUploadQuality quality,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null);

    /// <summary>
    /// Delete image from Cloudinary and mark as deleted in database
    /// </summary>
    /// <param name="publicId">Cloudinary public ID</param>
    /// <param name="hardDelete">True to permanently delete from database, False for soft delete</param>
    Task<bool> DeleteImageAsync(string publicId, bool hardDelete = false);

    /// <summary>
    /// Delete multiple images from Cloudinary and database
    /// </summary>
    Task<bool> DeleteMultipleImagesAsync(List<string> publicIds, bool hardDelete = false);

    /// <summary>
    /// Get image information from database or Cloudinary
    /// </summary>
    Task<CloudinaryImageInfo> GetImageInfoAsync(string publicId);

    /// <summary>
    /// Update image (delete old and upload new)
    /// </summary>
    Task<CloudinaryUploadResult> UpdateImageAsync(
        ImageUploadQuality quality,
        string oldPublicId,
        IFormFile newFile,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null,
        string description = null);

    /// <summary>
    /// Get all media assets by entity type and ID
    /// </summary>
    Task<List<MediaAsset>> GetMediaAssetsByEntityAsync(string entityType, Guid entityId);

    /// <summary>
    /// Get all media assets by user ID
    /// </summary>
    Task<List<MediaAsset>> GetMediaAssetsByUserAsync(Guid userId);

    /// <summary>
    /// Get total storage size used by user (in bytes)
    /// </summary>
    Task<int> GetUserStorageSizeAsync(Guid userId);
}

public interface IMediaAssetRepository
{
    Task<MediaAsset> CreateAsync(MediaAsset mediaAsset);
    Task<List<MediaAsset>> CreateMultipleAsync(List<MediaAsset> mediaAssets);
    Task<MediaAsset> GetByIdAsync(Guid id);
    Task<MediaAsset> GetByPublicIdAsync(string publicId);
    Task<List<MediaAsset>> GetByEntityAsync(string entityType, Guid entityId);
    Task<List<MediaAsset>> GetByUserIdAsync(Guid userId);
    Task<List<MediaAsset>> GetByProjectIdAsync(Guid projectId);
    Task<List<MediaAsset>> GetByTaskIdAsync(Guid taskId);
    Task<bool> UpdateAsync(MediaAsset mediaAsset);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> SoftDeleteByPublicIdAsync(string publicId);
    Task<bool> HardDeleteAsync(Guid id);
    Task<int> GetTotalSizeByUserAsync(Guid userId);
    Task<List<MediaAsset>> GetAllActiveAsync();
}

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Medo;
using Microsoft.Extensions.Options;
using server.Application.Common.Interfaces;
using server.Application.Enums;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Common.Models;
using server.Domain.Entities;

namespace server.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogManager _logger;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAssistantService _assistantService;

    public CloudinaryService(
        Cloudinary cloudinary,
        IOptions<CloudinarySettings> settings,
        ILogManager logger,
        IMediaAssetRepository mediaAssetRepository,
        IHttpContextAccessor httpContextAccessor,
        IAssistantService assistantService
        )
    {
        _cloudinary = cloudinary;
        _settings = settings.Value;
        _logger = logger;
        _mediaAssetRepository = mediaAssetRepository;
        _httpContextAccessor = httpContextAccessor;
        _assistantService = assistantService;
    }

    /// <summary>
    /// Uploads an image to Cloudinary and saves to database.
    /// </summary>
    public async Task<CloudinaryUploadResult> UploadImageAsync(
        IFormFile file,
        ImageUploadQuality quality = ImageUploadQuality.Standard,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null,
        string description = null)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Invalid file");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new BadRequestException($"Only image files are accepted: {string.Join(", ", allowedExtensions)}");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                throw new BadRequestException("File size cannot exceed 10MB");
            }

            // Create unique file name
            var fileName = $"{Uuid7.NewUuid7().ToGuid()}{fileExtension}";
            var folder = folderName ?? _settings.Folder_Name;

            var transformation = quality switch
            {
                ImageUploadQuality.Thumbnail => new Transformation()
                    .Width(200).Height(200).Crop("fill").Gravity("face")
                    .Quality("auto:low").FetchFormat("auto"),

                ImageUploadQuality.Low => new Transformation()
                    .Width(800).Height(800).Crop("limit")
                    .Quality(60).FetchFormat("auto"),

                ImageUploadQuality.Standard => new Transformation()
                    .Width(1200).Height(1200).Crop("limit")
                    .Quality("auto:good").FetchFormat("auto"),

                ImageUploadQuality.High => new Transformation()
                    .Width(2048).Height(2048).Crop("limit")
                    .Quality("auto:best").FetchFormat("auto"),

                ImageUploadQuality.Original => new Transformation()
                    .Quality("auto").FetchFormat("auto"),

                _ => new Transformation().Quality("auto:good").FetchFormat("auto")
            };

            // Configure upload parameters
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, file.OpenReadStream()),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = transformation,
                // UploadPreset = !string.IsNullOrEmpty(_settings.Preset_Name)
                //     ? _settings.Preset_Name
                //     : null
            };

            // Upload to Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.Error($"Cloudinary upload error: {uploadResult.Error.Message}");
                throw new BadRequestException($"Upload failed: {uploadResult.Error.Message}");
            }

            _logger.Info($"Upload successful to Cloudinary: {uploadResult.PublicId}");

            // Save to database
            var currentUserId = Guid.Parse(_assistantService.UserId);
            var mediaAsset = new MediaAsset
            {
                id = Uuid7.NewUuid7().ToGuid(),
                public_id = uploadResult.PublicId,
                url = uploadResult.Url.ToString(),
                secure_url = uploadResult.SecureUrl.ToString(),
                resource_type = uploadResult.ResourceType,
                format = uploadResult.Format,
                bytes = uploadResult.Bytes,
                width = uploadResult.Width,
                height = uploadResult.Height,
                user_id = currentUserId,
                entity_type = entityType,
                entity_id = entityId,
                description = description,
                created = DateTime.UtcNow,
                created_by = currentUserId,
                deleted = false,
                active = true
            };

            var savedAsset = await _mediaAssetRepository.CreateAsync(mediaAsset);
            _logger.Info($"Media asset saved to database with ID: {savedAsset.id}");

            // Return result
            return new CloudinaryUploadResult
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.Url.ToString(),
                SecureUrl = uploadResult.SecureUrl.ToString(),
                Format = uploadResult.Format,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Bytes = uploadResult.Bytes,
                ResourceType = uploadResult.ResourceType,
                CreatedAt = uploadResult.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error uploading image to Cloudinary {ex}");
            throw;
        }
    }

    /// <summary>
    /// Uploads multiple images at once and saves to database.
    /// </summary>
    public async Task<List<CloudinaryUploadResult>> UploadMultipleImagesAsync(
        List<IFormFile> files,
        ImageUploadQuality quality = ImageUploadQuality.Standard,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null)
    {
        try
        {
            if (files == null || !files.Any())
            {
                throw new BadRequestException("File list is empty");
            }

            var results = new List<CloudinaryUploadResult>();
            var mediaAssets = new List<MediaAsset>();
            var currentUserId = Guid.Parse(_assistantService.UserId);

            foreach (var file in files)
            {
                try
                {
                    // Validate file
                    if (file == null || file.Length == 0) continue;

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension)) continue;
                    if (file.Length > 10 * 1024 * 1024) continue;

                    // Upload to Cloudinary
                    var fileName = $"{Uuid7.NewUuid7().ToGuid()}{fileExtension}";
                    var folder = folderName ?? _settings.Folder_Name;
                    var transformation = quality switch
                    {
                        ImageUploadQuality.Thumbnail => new Transformation()
                            .Width(200).Height(200).Crop("fill").Gravity("face")
                            .Quality("auto:low").FetchFormat("auto"),

                        ImageUploadQuality.Low => new Transformation()
                            .Width(800).Height(800).Crop("limit")
                            .Quality(60).FetchFormat("auto"),

                        ImageUploadQuality.Standard => new Transformation()
                            .Width(1200).Height(1200).Crop("limit")
                            .Quality("auto:good").FetchFormat("auto"),

                        ImageUploadQuality.High => new Transformation()
                            .Width(2048).Height(2048).Crop("limit")
                            .Quality("auto:best").FetchFormat("auto"),

                        ImageUploadQuality.Original => new Transformation()
                            .Quality("auto").FetchFormat("auto"),

                        _ => new Transformation().Quality("auto:good").FetchFormat("auto")
                    };
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, file.OpenReadStream()),
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = false,
                        Transformation = transformation,
                        // UploadPreset = !string.IsNullOrEmpty(_settings.Preset_Name)
                        //     ? _settings.Preset_Name
                        //     : null
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        _logger.Error($"Failed to upload {file.FileName}: {uploadResult.Error.Message}");
                        continue;
                    }

                    // Prepare media asset for database
                    var mediaAsset = new MediaAsset
                    {
                        id = Uuid7.NewUuid7().ToGuid(),
                        public_id = uploadResult.PublicId,
                        url = uploadResult.Url.ToString(),
                        secure_url = uploadResult.SecureUrl.ToString(),
                        resource_type = uploadResult.ResourceType,
                        format = uploadResult.Format,
                        bytes = uploadResult.Bytes,
                        width = uploadResult.Width,
                        height = uploadResult.Height,
                        user_id = currentUserId,
                        entity_type = entityType,
                        entity_id = entityId,
                        created = DateTime.UtcNow,
                        created_by = currentUserId,
                        deleted = false,
                        active = true
                    };

                    mediaAssets.Add(mediaAsset);

                    results.Add(new CloudinaryUploadResult
                    {
                        PublicId = uploadResult.PublicId,
                        Url = uploadResult.Url.ToString(),
                        SecureUrl = uploadResult.SecureUrl.ToString(),
                        Format = uploadResult.Format,
                        Width = uploadResult.Width,
                        Height = uploadResult.Height,
                        Bytes = uploadResult.Bytes,
                        ResourceType = uploadResult.ResourceType,
                        CreatedAt = uploadResult.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to upload file: {file.FileName}, {ex}");
                    continue;
                }
            }

            // Batch save to database
            if (mediaAssets.Any())
            {
                await _mediaAssetRepository.CreateMultipleAsync(mediaAssets);
                _logger.Info($"Saved {mediaAssets.Count} media assets to database");
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error uploading multiple images: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Deletes an image by its PublicId from Cloudinary and marks as deleted in database.
    /// </summary>
    public async Task<bool> DeleteImageAsync(string publicId, bool hardDelete = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new BadRequestException("PublicId cannot be empty");
            }

            // Delete from Cloudinary
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result == "ok" || result.Result == "not found")
            {
                _logger.Info($"Image deleted from Cloudinary: {publicId}");

                if (hardDelete)
                {
                    var mediaAsset = await _mediaAssetRepository.GetByPublicIdAsync(publicId);
                    if (mediaAsset != null)
                    {
                        await _mediaAssetRepository.HardDeleteAsync(mediaAsset.id);
                        _logger.Info($"Media asset hard deleted from database: {publicId}");
                    }
                }
                else
                {
                    await _mediaAssetRepository.SoftDeleteByPublicIdAsync(publicId);
                    _logger.Info($"Media asset soft deleted in database: {publicId}");
                }

                return true;
            }

            _logger.Warn($"Failed to delete image: {publicId}. Result: {result.Result}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting image: {publicId}, {ex}");
            throw;
        }
    }

    /// <summary>
    /// Deletes multiple images from Cloudinary and database.
    /// </summary>
    public async Task<bool> DeleteMultipleImagesAsync(List<string> publicIds, bool hardDelete = false)
    {
        try
        {
            if (publicIds == null || !publicIds.Any())
            {
                throw new BadRequestException("PublicIds list is empty");
            }

            var allSuccess = true;

            foreach (var publicId in publicIds)
            {
                try
                {
                    var result = await DeleteImageAsync(publicId, hardDelete);
                    if (!result)
                    {
                        allSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to delete image: {publicId}, {ex}");
                    allSuccess = false;
                }
            }

            return allSuccess;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting multiple images: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Gets the detailed information of an image from database.
    /// </summary>
    public async Task<CloudinaryImageInfo> GetImageInfoAsync(string publicId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                throw new BadRequestException("PublicId cannot be empty");
            }

            // Get from database first
            var mediaAsset = await _mediaAssetRepository.GetByPublicIdAsync(publicId);

            if (mediaAsset != null)
            {
                return new CloudinaryImageInfo
                {
                    PublicId = mediaAsset.public_id,
                    Url = mediaAsset.url,
                    SecureUrl = mediaAsset.secure_url,
                    Format = mediaAsset.format,
                    Width = mediaAsset.width ?? 0,
                    Height = mediaAsset.height ?? 0,
                    Bytes = mediaAsset.bytes,
                    CreatedAt = mediaAsset.created
                };
            }

            // Fallback to Cloudinary API if not in database
            var getResourceParams = new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.GetResourceAsync(getResourceParams);

            if (result == null)
            {
                throw new Exception($"Image not found with PublicId: {publicId}");
            }

            return new CloudinaryImageInfo
            {
                PublicId = result.PublicId,
                Url = result.Url,
                SecureUrl = result.SecureUrl,
                Format = result.Format,
                Width = result.Width,
                Height = result.Height,
                Bytes = result.Bytes,
                CreatedAt = DateTime.Parse(result.CreatedAt),
                ImageMetadata = result.ImageMetadata
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error getting image info: {publicId}, {ex}");
            throw;
        }
    }

    /// <summary>
    /// Updates an image (deletes the old one and uploads the new one).
    /// </summary>
    public async Task<CloudinaryUploadResult> UpdateImageAsync(
        ImageUploadQuality quality,
        string oldPublicId,
        IFormFile newFile,
        string folderName = null,
        string entityType = null,
        Guid? entityId = null,
        string description = null)
    {
        try
        {
            // Upload the new image first
            var uploadResult = await UploadImageAsync(newFile, quality, folderName, entityType, entityId, description);

            // If the upload is successful, delete the old image
            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                try
                {
                    await DeleteImageAsync(oldPublicId, hardDelete: false);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to delete old image: {oldPublicId}, {ex}");
                }
            }

            return uploadResult;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating image: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Get media assets by entity
    /// </summary>
    public async Task<List<MediaAsset>> GetMediaAssetsByEntityAsync(string entityType, Guid entityId)
    {
        return await _mediaAssetRepository.GetByEntityAsync(entityType, entityId);
    }

    /// <summary>
    /// Get media assets by user
    /// </summary>
    public async Task<List<MediaAsset>> GetMediaAssetsByUserAsync(Guid userId)
    {
        return await _mediaAssetRepository.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Get total storage size used by user
    /// </summary>
    public async Task<int> GetUserStorageSizeAsync(Guid userId)
    {
        return await _mediaAssetRepository.GetTotalSizeByUserAsync(userId);
    }
}
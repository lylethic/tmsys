using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.Enums;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Services;

namespace server.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/cloudinary")]
    public class CloudinaryController : BaseApiController
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IAssistantService _assistantService;

        public CloudinaryController(
            ICloudinaryService cloudinaryService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger,
            IAssistantService assistantService
            )
        : base(mapper, httpContextAccessor, logger)
        {
            _cloudinaryService = cloudinaryService;
            _assistantService = assistantService;
        }

        /// <summary>
        /// Upload an image to Cloudinary and save to database
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(
            IFormFile file,
            ImageUploadQuality quality,
            [FromQuery] string folderName = null,
            [FromQuery] string entityType = null,
            [FromQuery] Guid? entityId = null,
            [FromQuery] string description = null)
        {
            try
            {
                if (file == null)
                {
                    return Error("Please select a file");
                }

                var result = await _cloudinaryService.UploadImageAsync(
                    file,
                    quality,
                    folderName,
                    entityType,
                    entityId,
                    description);

                return Success(new
                {
                    success = true,
                    message = "Upload successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in UploadImage endpoint", ex);
                return Error("Unexpected error while uploading image");
            }
        }

        /// <summary>
        /// Upload multiple images to Cloudinary and save to database
        /// </summary>
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages(
            List<IFormFile> files,
            ImageUploadQuality quality,
            [FromQuery] string folderName = null,
            [FromQuery] string entityType = null,
            [FromQuery] Guid? entityId = null)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return Error("Please select at least one file");
                }

                var results = await _cloudinaryService.UploadMultipleImagesAsync(
                    files,
                    quality,
                    folderName,
                    entityType,
                    entityId);

                return Success(new
                {
                    isSuccess = true,
                    message = $"Upload thành công {results.Count}/{files.Count} file",
                    data = results,
                    totalUploaded = results.Count,
                    totalRequested = files.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in UploadMultipleImages endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Delete an image from Cloudinary and database
        /// </summary>
        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteImage(
            string publicId,
            [FromQuery] bool hardDelete = false)
        {
            try
            {
                publicId = Uri.UnescapeDataString(publicId);
                var result = await _cloudinaryService.DeleteImageAsync(publicId, hardDelete);

                if (result)
                {
                    return Success(hardDelete ? "Deleted permanently" : "Deleted successfully");
                }

                return Error("Delete failed");
            }
            catch (Exception ex)
            {
                _logger.Error("Error in DeleteImage endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Delete multiple images from Cloudinary and database
        /// </summary>
        [HttpDelete("delete-multiple")]
        public async Task<IActionResult> DeleteMultipleImages(
            [FromBody] List<string> publicIds,
            [FromQuery] bool hardDelete = false)
        {
            try
            {
                if (publicIds == null || !publicIds.Any())
                {
                    return Error("Empty list of publicIds!");
                }

                var result = await _cloudinaryService.DeleteMultipleImagesAsync(publicIds, hardDelete);

                return Success(new
                {
                    success = result,
                    message = result ? "Delete successfully" : "Some of images were not found or deleted"
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in DeleteMultipleImages endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get image information from database or Cloudinary
        /// </summary>
        [HttpGet("{publicId}/info")]
        public async Task<IActionResult> GetImageInfo(string publicId)
        {
            try
            {
                publicId = Uri.UnescapeDataString(publicId);
                var result = await _cloudinaryService.GetImageInfoAsync(publicId);

                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetImageInfo endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Update an image (delete old, upload new)
        /// </summary>
        [HttpPut("{oldPublicId}")]
        public async Task<IActionResult> UpdateImage(
            string oldPublicId,
            IFormFile newFile,
            ImageUploadQuality quality,
            [FromQuery] string folderName = null,
            [FromQuery] string entityType = null,
            [FromQuery] Guid? entityId = null,
            [FromQuery] string description = null)
        {
            try
            {
                if (newFile == null)
                {
                    return Error("Please select a new file");
                }

                oldPublicId = Uri.UnescapeDataString(oldPublicId);
                var result = await _cloudinaryService.UpdateImageAsync(
                    quality,
                    oldPublicId,
                    newFile,
                    folderName,
                    entityType,
                    entityId,
                    description);

                return Success(new
                {
                    success = true,
                    message = "Update successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in UpdateImage endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get all media assets by entity (project, task, etc.)
        /// </summary>
        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetMediaAssetsByEntity(string entityType, Guid entityId)
        {
            try
            {
                var results = await _cloudinaryService.GetMediaAssetsByEntityAsync(entityType, entityId);

                return Success(new
                {
                    entityType,
                    entityId,
                    total = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetMediaAssetsByEntity endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get all media assets by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMediaAssetsByUser(Guid userId)
        {
            try
            {
                var results = await _cloudinaryService.GetMediaAssetsByUserAsync(userId);

                return Success(new
                {
                    userId,
                    total = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetMediaAssetsByUser endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get total storage size used by user
        /// </summary>
        [HttpGet("user/{userId}/storage")]
        public async Task<IActionResult> GetUserStorageSize(Guid userId)
        {
            try
            {
                var totalBytes = await _cloudinaryService.GetUserStorageSizeAsync(userId);
                var totalMB = totalBytes / (1024.0 * 1024.0);

                return Success(new
                {
                    userId,
                    totalBytes,
                    totalMB = Math.Round(totalMB, 2),
                    totalGB = Math.Round(totalMB / 1024.0, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetUserStorageSize endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get current user's media assets
        /// </summary>
        [HttpGet("my-images")]
        public async Task<IActionResult> GetMyImages()
        {
            try
            {
                var currentUserId = Guid.Parse(_assistantService.UserId);

                var results = await _cloudinaryService.GetMediaAssetsByUserAsync(currentUserId);

                return Success(new
                {
                    total = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetMyImages endpoint", ex);
                return Error("Unexpected error");
            }
        }

        /// <summary>
        /// Get current user's storage usage
        /// </summary>
        [HttpGet("my-storage")]
        public async Task<IActionResult> GetMyStorage()
        {
            try
            {
                var currentUserId = Guid.Parse(_assistantService.UserId);

                var totalBytes = await _cloudinaryService.GetUserStorageSizeAsync(currentUserId);
                var totalMB = totalBytes / (1024.0 * 1024.0);

                return Success(new
                {
                    totalBytes,
                    totalMB = Math.Round(totalMB, 2),
                    totalGB = Math.Round(totalMB / 1024.0, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetMyStorage endpoint", ex);
                return Error("Unexpected error");
            }
        }
    }
}

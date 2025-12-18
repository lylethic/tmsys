using System.Text.Json;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.DTOs;
using server.Application.Models;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Providers;
using server.Common.Settings;
using server.Repositories;
using server.Services;

namespace server.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/notis")]
    public class NotificationController : BaseApiController
    {
        private readonly NotificationService _notificationService;
        private readonly NotificationRepository _notificationRepo;
        private readonly INotificationCategoryProvider _categoryProvider;
        private readonly IAssistantService _assistantService;
        public NotificationController(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger,
            IServiceProvider service,
            INotificationCategoryProvider categoryProvider,
            IAssistantService assistantService) : base(mapper, httpContextAccessor, logger)
        {
            this._notificationService = service.GetRequiredService<NotificationService>();
            this._notificationRepo = service.GetRequiredService<NotificationRepository>();
            this._categoryProvider = categoryProvider;
            this._assistantService = assistantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] NotificationSearchRequest request)
        {
            try
            {
                var result = await _notificationService.GetAllAsync(request);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _notificationService.GetNotificationByIdAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Get user's notification list based on user ID extract from token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications(PaginationRequest request)
        {
            try
            {
                var userID = Guid.Parse(_assistantService.UserId);
                var result = await _notificationRepo.GetMyNotificationsAsync(userID, request);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Get notifications with user, status
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("notis")]
        public async Task<IActionResult> GetNotis(PaginationRequest request)
        {
            try
            {
                var repoResult = await _notificationRepo.GetNotificationsAsync(request);

                var cleanData = repoResult.Data.Select(rawDto => new ExtendNotification
                {
                    Notifications = JsonSerializer.Deserialize<List<NotificationObject>>(rawDto.Notifications, _jsonOptions),
                    Extend_user = JsonSerializer.Deserialize<UserObject>(rawDto.Extend_user, _jsonOptions),
                    Extend_status = JsonSerializer.Deserialize<StatusObject>(rawDto.Extend_status, _jsonOptions)
                }).ToList();

                var finalResponse = new PaginatedResult<ExtendNotification>
                {
                    TotalCount = repoResult.TotalCount,
                    Page = request.PageIndex,
                    PageSize = request.PageSize,
                    Data = cleanData
                };
                return Success(finalResponse);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationDto dto)
        {
            try
            {
                var result = await _notificationService.CreateNotificationAsync(dto);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] NotificationDto dto)
        {
            try
            {
                var result = await _notificationService.UpdateNotificationAsync(id, dto);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPatch("markAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var result = await _notificationService.MarkAllAsReadAsync(Guid.Parse(_assistantService.UserId));
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _notificationService.SoftDeleteNotificationAsync(id);
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> BulkDeleteAsync([FromBody] List<Guid> ids)
        {
            try
            {
                var result = await _notificationService.BulkDeleteAsync(ids);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            try
            {
                return Success(_categoryProvider.Catalog);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true // Ignore case sensitivity
        };
    }
}

using System;
using System.Threading.Tasks;
using Medo;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.Extensions.Caching.Memory;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Repositories;
using server.Services;

namespace server.Common.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _ignorePrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/health",
            "/swagger",
            "/favicon.ico",
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/users/me",
            "/api/v1/users/profile",
            "/api/v1/auth/send-code",
            "/api/v1/auth/confirm-reset-password",
        };
        public RequestLoggingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, ClientRequestLogRepository repository, IAssistantService assistant, ILogger<RequestLoggingMiddleware> logger)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            var trimmedPath = path.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase)
            ? path.Substring("/api/v1/".Length)
            : path;

            // Bỏ qua nếu URL nằm trong danh sách ignore
            if (_ignorePrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.ContainsKey("Authorization") ||
                context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                await _next(context);
                logger.LogInformation("Skipping request log: no valid Authorization header or user not authenticated");
                return;
            }

            string userId = null;
            string sessionId = null;
            try
            {
                userId = assistant.UserId;
                sessionId = assistant.SessionId;
            }
            catch (BadRequestException)
            {

            }

            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ip) && context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
            }

            // ===== Add anti-continuous logging mechanism =====
            var cacheKey = $"{userId}:{sessionId}:{path}:{context.Request.Method}";
            if (_cache.TryGetValue(cacheKey, out _))
            {
                await _next(context);
                return;
            }

            var log = new Client_request_log
            {
                id = Uuid7.NewUuid7().ToGuid(),
                User_id = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                Session_id = string.IsNullOrEmpty(sessionId) ? null : Guid.Parse(sessionId),
                Client_ip = ip,
                User_agent = context.Request.Headers.UserAgent.ToString(),
                Url = trimmedPath.ToLowerInvariant().TrimEnd('/'),
                Method = context.Request.Method,
                Created = DateTime.UtcNow
            };

            await repository.CreateItemAsync(log);

            // Save cache in 15s
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(15));

            await _next(context);
        }
    }
}

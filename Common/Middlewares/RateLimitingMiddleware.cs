using System;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using server.Common.Models;

namespace server.Common.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly RateLimitRuleModel _rule;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, RateLimitRuleModel rule)
    {
        _next = next;
        _cache = cache;
        _rule = rule;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var identifier = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var cacheKey = $"RateLimit_{identifier}";

        if (!_cache.TryGetValue(cacheKey, out int requestCount))
        {
            requestCount = 0;
            _cache.Set(cacheKey, requestCount, _rule.Window);
        }

        if (requestCount >= _rule.Limit)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new { Message = "Rate limit exceeded. Try again later." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        _cache.Set(cacheKey, ++requestCount, _rule.Window);
        await _next(context);
    }
}

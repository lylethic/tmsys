using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class ClientRequestLogRepository(IDbConnection connection, ILogManager logger, IHttpContextAccessor contextAccessor) : SimpleCrudRepository<Client_request_log, Guid>(connection)
{
    private readonly ILogManager _logger = logger;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    public async Task CreateItemAsync(Client_request_log entity)
    {
        try
        {
            entity.Id = Uuid7.NewUuid7().ToGuid();
            const string sql = @"
                INSERT INTO client_request_log
                (id, user_id, session_id, client_ip, user_agent, url, method, created)
                VALUES (@Id, @User_id, @Session_id, @Client_ip, @User_agent, @Url, @Method, @Created);";

            await _connection.ExecuteAsync(sql, entity);
        }
        catch (Exception ex)
        {
            _logger.Error($"Add tracking request failed {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<FeatureUsageDto>> GetMostAccessedFeaturesAsync(DateTime? from = null, DateTime? to = null)
    {
        from ??= DateTime.UtcNow.AddDays(-7);
        to ??= DateTime.UtcNow;

        const string sql = """
            SELECT 
                url AS Url,
                COUNT(*) AS Count
            FROM client_request_log
            WHERE created BETWEEN @From AND @To
            GROUP BY url
            ORDER BY COUNT(*) DESC;
        """;

        return await _connection.QueryAsync<FeatureUsageDto>(sql, new { From = from, To = to });
    }

    protected string GetIpAddress()
    {
        string ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        // If behind proxy/load balancer (e.g. Nginx, Cloudflare), check headers
        if (string.IsNullOrEmpty(ip) && _contextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ip = _contextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
        }
        if (ip is null)
        {
            _logger.Warn($"Get IP address error");
            return "";
        }
        return ip;
    }
}

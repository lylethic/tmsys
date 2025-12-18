using System;
using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using server.Application.DTOs;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/test")]
public class StatisticsController : BaseApiController
{
    private readonly ClientRequestLogRepository _repository;
    public StatisticsController(
        IServiceProvider provider,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _repository = provider.GetRequiredService<ClientRequestLogRepository>();
    }

    /// <summary>
    /// Creates a new client request log.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client_request_logCreate dto)
    {
        var entity = _mapper.Map<Client_request_log>(dto);
        await _repository.CreateAsync(entity);
        _logger.Info($"Created client request log with id {entity.ToJson()}");
        return Success("Created successfully");
    }


    [HttpGet("ip")]
    public IActionResult GetClientIp()
    {
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // If behind proxy/load balancer (e.g. Nginx, Cloudflare), check headers
        if (string.IsNullOrEmpty(ip) && HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ip = HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
        }

        return Success(new { IpAddress = ip });
    }

    [HttpGet("most-access")]
    public async Task<IActionResult> GetMostAccessedFeatures()
    {
        var result = await _repository.GetMostAccessedFeaturesAsync();
        return Success(result);
    }
}

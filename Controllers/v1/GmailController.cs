using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Models;
using server.Common.Settings;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/gmail")]
public class GmailController : BaseApiController
{
    private readonly IMailService _gmailService;
    public GmailController(
                IMailService gmailService,
                IMapper mapper,
                IHttpContextAccessor httpContextAccessor,
                ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        this._gmailService = gmailService;
    }
    [HttpPost]
    public async Task<IActionResult> SendEmail(SendEmailRequest request)
    {
        await _gmailService.SendEmailAsync(request);
        return Ok(new { status = 200, message = "Email sent successfully" });
    }

    [HttpGet("test-config")]
    public IActionResult TestConfig([FromServices] IOptions<GmailOptions> options)
    {
        var config = options.Value;
        return Ok(new
        {
            Host = config.Host,
            Port = config.Port,
            Email = config.Email,
            HasPassword = !string.IsNullOrEmpty(config.Password)
        });
    }
}

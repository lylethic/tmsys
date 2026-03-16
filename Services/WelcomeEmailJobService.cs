using server.Application.Request;
using server.Common.Interfaces;

namespace server.Services;

public class WelcomeEmailJobService
{
    private readonly IMailService _mailService;
    private readonly ILogManager _logManager;

    public WelcomeEmailJobService(IMailService mailService, ILogManager logManager)
    {
        _mailService = mailService;
        _logManager = logManager;
    }

    public async Task SendWelcomeEmailAsync(string email, string? name)
    {
        var body = await EmailTemplateManager.GetWelcomeEmailAsync(email, name ?? "User");
        var emailRequest = new SendEmailRequest(email, "Welcome to Loopy!", body);

        await _mailService.SendEmailAsync(emailRequest);
        _logManager.Info($"Welcome email sent successfully to {email}");
    }
}

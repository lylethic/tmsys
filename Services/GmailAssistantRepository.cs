using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Models;

namespace server.Services;

public class GmailAssistantService : IMailService
{
    private readonly GmailOptions _options;

    public GmailAssistantService(IOptions<GmailOptions> gmailOptions)
    {
        this._options = gmailOptions.Value;
    }

    public void SendEmail()
    {
        Console.WriteLine($"Host: {_options.Host}, Port: {_options.Port}");
    }

    public async Task SendEmailAsync(SendEmailRequest request)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.Email),
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(request.Recipient));

        using var smtpClient = new SmtpClient
        {
            Host = _options.Host,
            Port = _options.Port,
            Credentials = new NetworkCredential(
            _options.Email, _options.Password),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mailMessage);
    }
}

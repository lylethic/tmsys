using System;
using server.Application.Request;

namespace server.Common.Interfaces;

public interface IMailService
{
    Task SendEmailAsync(SendEmailRequest request);

}

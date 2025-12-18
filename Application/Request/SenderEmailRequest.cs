namespace server.Application.Request;

public record SendEmailRequest(string Recipient, string Subject, string Body);


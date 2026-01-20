namespace server.Application.DTOs;

public sealed class MenteeDto
{
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Monday { get; set; }

    public string? Tuesday { get; set; }

    public string? Wednesday { get; set; }

    public string? Thursday { get; set; }

    public string? Friday { get; set; }
    public DateTimeOffset? Week_start { get; set; }
    public DateTimeOffset? Week_end { get; set; }
}

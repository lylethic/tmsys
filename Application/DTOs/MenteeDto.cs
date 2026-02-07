namespace server.Application.DTOs;

public sealed class MenteeDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Monday { get; set; }

    public string? Tuesday { get; set; }

    public string? Wednesday { get; set; }

    public string? Thursday { get; set; }

    public string? Friday { get; set; }
    public string? Week_start { get; set; }
    public string? Week_end { get; set; }
}

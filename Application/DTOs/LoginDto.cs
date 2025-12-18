using System;

namespace server.Application.DTOs;

public class LoginDto
{
    // public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}

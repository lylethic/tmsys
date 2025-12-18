using System;

namespace server.Application.Request;

public class AuthRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

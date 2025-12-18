using System;
using server.Common.Domain.Entities;

namespace server.Application.Models;

public class UserModel : DomainModel
{
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Profilepic { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime Last_login_time { get; set; }
    public bool? Is_send_email { get; set; }
}

public class ExtendUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Profilepic { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool? Active { get; set; }
}

using server.Common.Domain.Request.Update;
using server.Common.Interfaces;
using System.Text.Json.Serialization;

namespace server.Application.DTOs;

public class UserDto
{
    /// <summary>
    /// Name of the user
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Email of the user
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Password of the user. At least 6 charaters
    /// </summary>
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }

    /// <summary>
    /// Avatar.
    /// AllowNull
    /// </summary>
    public IFormFile? ProfilePic { get; set; } = null;

    /// <summary>
    /// City where the user lives in.
    /// AllowNull
    /// </summary>
    public string? City { get; set; } = null;

    [JsonIgnore]
    public Guid? Created_By { get; set; } = null;

    [JsonIgnore]
    public bool Active { get; set; } = true;

    public Guid Role_Id { get; set; }
}

public class CreateUserDto
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public string? ProfilePic { get; set; } = null;

    public string? City { get; set; } = null;
}

public class User_Permisson_Dto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public bool Active { get; set; }
    public Guid? Role_id { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public record UpdateUserDto(Guid Role_id, string Email, bool active);

public class UserUpdate : DomainUpdate
{
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? City { get; set; } = null;
    public bool? Is_send_email { get; set; }
    public string? ProfilePic { get; set; } = null;
}
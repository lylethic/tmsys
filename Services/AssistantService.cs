using System.Security.Claims;
using server.Common.Exceptions;

namespace server.Services;

/// <summary>
/// Provides access to the current assistant's information.
/// UserId,sessionId, email, role, permissions[]
/// </summary>
public interface IAssistantService
{
    string UserId { get; }
    string SessionId { get; }
    string Email { get; }
    string Role { get; }
    List<string> Permissions { get; }
}

public class AssistantService : IAssistantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal _user;

    public AssistantService(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
        this._user = _httpContextAccessor.HttpContext?.User;
    }

    public string UserId =>
       _user?.FindFirst("user_id")?.Value ?? throw new BadRequestException("Invalid user");

    public string SessionId =>
    _user?.FindFirst("session_id")?.Value ?? throw new BadRequestException("Invalid user session");

    public string Email =>
        _user?.FindFirst(ClaimTypes.Email)?.Value ?? throw new BadRequestException("Invalid email");

    public string Role =>
        _user?.FindFirst(ClaimTypes.Role)?.Value ?? throw new BadRequestException("Invalid role");

    public List<string> Permissions =>
        _user?.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList() ?? [];
}

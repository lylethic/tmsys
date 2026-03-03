namespace server.Application.DTOs;

// ── RoleMenu ───────────────────────────────────────────────────────────────

public sealed class RoleMenuCreateDto
{
    public required Guid RoleId { get; set; }
    public Guid? MenuId { get; set; }
    public string? ControllerName { get; set; }
    public bool CanView { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
}

public sealed class RoleMenuUpdateDto
{
    public bool CanView { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
}

public sealed class RoleMenuResponseDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public Guid? MenuId { get; set; }
    public string? MenuName { get; set; }
    public string? ControllerName { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanExport { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

/// <summary>
/// Full menu tree for a specific role, used to build dynamic navigation.
/// </summary>
public sealed class DynamicMenuDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public string? GroupIcon { get; set; }
    public int GroupSort { get; set; }
    public Guid? ParentGroupId { get; set; }
    public List<DynamicMenuItemDto> Items { get; set; } = [];
}

public sealed class DynamicMenuItemDto
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = null!;
    public string Controller { get; set; } = null!;
    public string ControllerName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public int Sort { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanExport { get; set; }
}

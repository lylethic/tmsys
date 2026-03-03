namespace server.Application.DTOs;

// ── SysMenu ────────────────────────────────────────────────────────────────

public sealed class SysMenuCreateDto
{
    public required string ControllerName { get; set; }
    public required string Controller { get; set; }
    public required string Action { get; set; }
    public required string Name { get; set; }
    public string? Name_vn { get; set; }
    public required Guid MenuGroupId { get; set; }
    public int Sort { get; set; } = 0;
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
    public bool Active { get; set; } = true;
    public bool IsShowMenu { get; set; } = true;
}

public sealed class SysMenuUpdateDto
{
    public required string ControllerName { get; set; }
    public required string Controller { get; set; }
    public required string Action { get; set; }
    public required string Name { get; set; }
    public string? Name_vn { get; set; }
    public required Guid MenuGroupId { get; set; }
    public int Sort { get; set; } = 0;
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
    public bool Active { get; set; } = true;
    public bool IsShowMenu { get; set; } = true;
}

public sealed class SysMenuResponseDto
{
    public Guid Id { get; set; }
    public string ControllerName { get; set; } = null!;
    public string Controller { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Name_vn { get; set; }
    public Guid MenuGroupId { get; set; }
    public string? MenuGroupName { get; set; }
    public int Sort { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanExport { get; set; }
    public bool Active { get; set; }
    public bool IsShowMenu { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

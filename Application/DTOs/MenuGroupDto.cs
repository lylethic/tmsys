namespace server.Application.DTOs;

// ── MenuGroup ──────────────────────────────────────────────────────────────

public sealed class MenuGroupCreateDto
{
    public required string Name { get; set; }
    public string? Name_vn { get; set; }
    public int Sort { get; set; } = 0;
    public string? Icon { get; set; }
    public bool Active { get; set; } = true;
    public Guid? ParentId { get; set; }
}

public sealed class MenuGroupUpdateDto
{
    public required string Name { get; set; }
    public string? Name_vn { get; set; }
    public int Sort { get; set; } = 0;
    public string? Icon { get; set; }
    public bool Active { get; set; } = true;
    public Guid? ParentId { get; set; }
}

public sealed class MenuGroupResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Name_vn { get; set; }
    public int Sort { get; set; }
    public string? Icon { get; set; }
    public bool Active { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public List<MenuGroupResponseDto> Children { get; set; } = [];
    public List<SysMenuResponseDto> Menus { get; set; } = [];
}

#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

/// <summary>
/// System menu item
/// Maps to SYS_MENU | Table: sys_menus
/// </summary>
[Table("sys_menus")]
public class SysMenu : SystemLogEntity<Guid>
{
    /// <summary>Display name used in permission check (e.g. "Tasks")</summary>
    public string ControllerName { get; set; } = null!;

    /// <summary>ASP.NET controller class name (e.g. "TasksController")</summary>
    public string Controller { get; set; } = null!;

    /// <summary>Default action (e.g. "Index")</summary>
    public string Action { get; set; } = null!;

    /// <summary>Display name shown in UI</summary>
    public string Name { get; set; } = null!;
    public string? Name_vn { get; set; }

    public Guid MenuGroupId { get; set; }
    public int Sort { get; set; } = 0;

    // Permission flags: describes which operations this menu SUPPORTS
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
    public bool IsShowMenu { get; set; } = true;
}

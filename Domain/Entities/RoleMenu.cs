#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

/// <summary>
/// Role ↔ Menu permission assignment
/// Maps to PHANQUYEN | Table: role_menus
/// </summary>
[Table("role_menus")]
public class RoleMenu : SystemLogEntity<Guid>
{
    public Guid RoleId { get; set; }
    public Guid? MenuId { get; set; }

    /// <summary>Denormalised for fast permission lookup by controller name</summary>
    public string ControllerName { get; set; }

    // Granted flags
    public bool CanView { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;
    public bool CanExport { get; set; } = false;
}

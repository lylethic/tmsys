#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

/// <summary>
/// Menu group / navigation group  
/// Maps to PHANQUYEN_NHOMQUYEN | Table: menu_groups
/// </summary>
[Table("menu_groups")]
public class MenuGroup : SystemLogEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string? Name_vn { get; set; }
    public int Sort { get; set; } = 0;
    public string Icon { get; set; }
    public Guid? ParentId { get; set; }
}

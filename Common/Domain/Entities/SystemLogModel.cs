using System;

namespace server.Common.Domain.Entities;

public abstract class SystemLogModel<T> : BaseEntity<T>
{
    public bool Active { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public DateTime? Created { get; set; } = null;
    public DateTime? Updated { get; set; } = null;
    public string? Created_by { get; set; } = null;
    public string? Updated_by { get; set; } = null;
}

namespace server.Common.Domain.Entities;

public abstract class SystemLogEntity<T> : BaseEntity<T>
{
    public bool? Active { get; set; } = true;
    public bool? Deleted { get; set; } = false;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public Guid? Created_by { get; set; } = null;
    public Guid? Updated_by { get; set; } = null;
}

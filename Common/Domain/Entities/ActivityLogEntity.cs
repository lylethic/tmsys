namespace server.Common.Domain.Entities;

public class ActivityLogEntity<T> : SystemLogEntity<T>
{
    public string Created_By { get; set; } = string.Empty;
    public string Updated_By { get; set; } = string.Empty;
}

using System;
using System.Text.Json.Serialization;

namespace server.Common.Domain.Request.Create;

public class DomainCreate
{
    [JsonIgnore]
    public Guid Created_by { get; set; }
    [JsonIgnore]
    public bool Deleted { get; set; } = false;
    [JsonIgnore]
    public bool Active { get; set; } = true;
    [JsonIgnore]
    public DateTime Created { get; set; } = DateTime.UtcNow;
    [JsonIgnore]
    public DateTime Updated { get; set; } = DateTime.UtcNow;
}

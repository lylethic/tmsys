using System;
using System.Text.Json.Serialization;

namespace server.Common.Domain.Entities;

public class DomainModel
{
    public Guid Id { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
    public Guid? Created_by { get; set; }
    public Guid? Updated_by { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }

    [JsonIgnore]
    public long? Total_count { get; set; }
}

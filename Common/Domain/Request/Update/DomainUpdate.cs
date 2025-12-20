using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Common.Domain.Request.Update;

public class DomainUpdate
{
    [JsonIgnore]
    [Column("deleted")]
    public bool? Deleted { get; set; } = false;

    [JsonIgnore]
    [Column("active")]
    public bool? Active { get; set; } = true;

    [JsonIgnore]
    [Column("updated")]
    public DateTime? Updated { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    [Column("updated_by")]
    public Guid? Updated_by { get; set; }
}

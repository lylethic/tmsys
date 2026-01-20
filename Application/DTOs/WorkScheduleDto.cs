using System;
using System.Text.Json.Serialization;
using server.Common.Json;

namespace server.Application.DTOs;

public class WorkScheduleDto
{
    public Guid? Intern_id { get; set; }

    public string? Intern_email { get; set; }

    public string? Mentor_email { get; set; }

    [JsonConverter(typeof(DateOnlyDateTimeOffsetConverter))]
    public DateTimeOffset? Week_start { get; set; }

    [JsonConverter(typeof(DateOnlyDateTimeOffsetConverter))]
    public DateTimeOffset? Week_end { get; set; }

    public string? Monday { get; set; }

    public string? Tuesday { get; set; }

    public string? Wednesday { get; set; }

    public string? Thursday { get; set; }

    public string? Friday { get; set; }

    public string? Full_name { get; set; }
}

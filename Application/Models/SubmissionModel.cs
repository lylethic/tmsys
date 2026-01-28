using System;
using System.Text.Json.Serialization;
using server.Common.Domain.Entities;

namespace server.Application.Models;

public class SubmissionModel : Domain.Entities.Submission
{
    [JsonPropertyName("extend_task")]
    public object? Extend_task { get; set; } = null;

    [JsonPropertyName("extend_user")]
    public object? Extend_user { get; set; } = null;
}

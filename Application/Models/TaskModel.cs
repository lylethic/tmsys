using System;
using System.Text.Json.Serialization;
using server.Common.Domain.Entities;

namespace server.Application.Models;

public class TaskModel : Domain.Entities.Tasks
{
    [JsonPropertyName("extend_project")]
    public object? Extend_project { get; set; } = null;
    [JsonPropertyName("extend_user")]
    public object? Extend_user { get; set; } = null;
}

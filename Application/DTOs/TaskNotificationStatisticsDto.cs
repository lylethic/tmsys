using System;

namespace server.Application.DTOs;

/// <summary>
/// DTO for task deadline notification statistics
/// </summary>
public class TaskNotificationStatisticsDto
{
  public int TotalTasksChecked { get; set; }
  public int TasksDueToday { get; set; }
  public int TasksOverdue { get; set; }
  public int NotificationsCreated { get; set; }
  public int NotificationsSent { get; set; }
  public DateTime CheckedAt { get; set; }
  public double ExecutionTimeSeconds { get; set; }
}

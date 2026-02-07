using System;
using System.Text.Json.Serialization;

namespace server.Application.DTOs;

// ──────────────────────────────────────────────
// 1. TASK STATISTICS
// ──────────────────────────────────────────────

/// <summary>
/// Task overview: total, by status, overdue, average score.
/// </summary>
public class TaskOverviewDto
{
    [JsonPropertyName("total_tasks")]
    public int TotalTasks { get; set; }

    [JsonPropertyName("by_status")]
    public List<StatusCountDto> ByStatus { get; set; } = [];

    [JsonPropertyName("overdue_tasks")]
    public int OverdueTasks { get; set; }

    [JsonPropertyName("completed_on_time")]
    public int CompletedOnTime { get; set; }

    [JsonPropertyName("completed_late")]
    public int CompletedLate { get; set; }

    [JsonPropertyName("avg_completion_days")]
    public double AvgCompletionDays { get; set; }
}

public class StatusCountDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Task statistics by member.
/// </summary>
public class MemberTaskStatsDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("total_tasks")]
    public int TotalTasks { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("in_progress")]
    public int InProgress { get; set; }

    [JsonPropertyName("overdue")]
    public int Overdue { get; set; }

    [JsonPropertyName("avg_score")]
    public decimal? AvgScore { get; set; }

    [JsonPropertyName("on_time_rate")]
    public double OnTimeRate { get; set; }
}

/// <summary>
/// Task statistics by project.
/// </summary>
public class ProjectTaskStatsDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

    [JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("total_tasks")]
    public int TotalTasks { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("in_progress")]
    public int InProgress { get; set; }

    [JsonPropertyName("overdue")]
    public int Overdue { get; set; }

    [JsonPropertyName("completion_rate")]
    public double CompletionRate { get; set; }
}

// ──────────────────────────────────────────────
// 2. ATTENDANCE STATISTICS
// ──────────────────────────────────────────────

/// <summary>
/// Attendance overview: total check-ins, valid, invalid, by day.
/// </summary>
public class AttendanceOverviewDto
{
    [JsonPropertyName("total_checkins")]
    public int TotalCheckins { get; set; }

    [JsonPropertyName("valid_checkins")]
    public int ValidCheckins { get; set; }

    [JsonPropertyName("invalid_checkins")]
    public int InvalidCheckins { get; set; }

    [JsonPropertyName("valid_rate")]
    public double ValidRate { get; set; }

    [JsonPropertyName("unique_users")]
    public int UniqueUsers { get; set; }

    [JsonPropertyName("daily_trend")]
    public List<DailyCheckinDto> DailyTrend { get; set; } = [];
}

public class DailyCheckinDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("valid")]
    public int Valid { get; set; }

    [JsonPropertyName("invalid")]
    public int Invalid { get; set; }
}

/// <summary>
/// Attendance statistics by member.
/// </summary>
public class MemberAttendanceStatsDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("total_checkins")]
    public int TotalCheckins { get; set; }

    [JsonPropertyName("valid_checkins")]
    public int ValidCheckins { get; set; }

    [JsonPropertyName("invalid_checkins")]
    public int InvalidCheckins { get; set; }

    [JsonPropertyName("valid_rate")]
    public double ValidRate { get; set; }

    [JsonPropertyName("working_days")]
    public int WorkingDays { get; set; }

    [JsonPropertyName("attendance_rate")]
    public double AttendanceRate { get; set; }
}

// ──────────────────────────────────────────────
// 3. WORK SCHEDULE STATISTICS
// ──────────────────────────────────────────────

/// <summary>
/// Work schedule overview: day distribution, total registered interns, mentor load.
/// </summary>
public class WorkScheduleOverviewDto
{
    [JsonPropertyName("total_schedules")]
    public int TotalSchedules { get; set; }

    [JsonPropertyName("total_interns")]
    public int TotalInterns { get; set; }

    [JsonPropertyName("total_mentors")]
    public int TotalMentors { get; set; }

    [JsonPropertyName("day_distribution")]
    public DayDistributionDto DayDistribution { get; set; } = new();
}

public class DayDistributionDto
{
    [JsonPropertyName("monday")]
    public int Monday { get; set; }
    [JsonPropertyName("tuesday")]
    public int Tuesday { get; set; }
    [JsonPropertyName("wednesday")]
    public int Wednesday { get; set; }
    [JsonPropertyName("thursday")]
    public int Thursday { get; set; }
    [JsonPropertyName("friday")]
    public int Friday { get; set; }
}

/// <summary>
/// Mentor load statistics: how many interns each mentor manages.
/// </summary>
public class MentorLoadDto
{
    [JsonPropertyName("mentor_email")]
    public string MentorEmail { get; set; } = string.Empty;

    [JsonPropertyName("intern_count")]
    public int InternCount { get; set; }
}

// ──────────────────────────────────────────────
// 4. COMPREHENSIVE DASHBOARD
// ──────────────────────────────────────────────

/// <summary>
/// Comprehensive dashboard of all main statistics.
/// </summary>
public class DashboardDto
{
    [JsonPropertyName("tasks")]
    public TaskOverviewDto Tasks { get; set; } = new();

    [JsonPropertyName("attendance")]
    public AttendanceOverviewDto Attendance { get; set; } = new();

    [JsonPropertyName("work_schedule")]
    public WorkScheduleOverviewDto WorkSchedule { get; set; } = new();
}

// using Asp.Versioning;
// using Hangfire;
// using Hangfire.Storage;
// using Microsoft.AspNetCore.Mvc;
// using server.Common.Interfaces;
// using server.notifications;
// using server.Services;

// namespace server.Controllers.v1
// {
//   [ApiVersion("1.0")]
//   [ApiController]
//   [Route("v{version:apiVersion}/jobs")]
//   public class JobController(IJobRunService jobRunService, ILogManager logger, NotificationService notificationService) : ControllerBase
//   {
//     private readonly ILogManager _logger = logger;
//     private readonly IJobRunService _jobRunService = jobRunService;
//     private readonly NotificationService _notificationService = notificationService;
//     [HttpPost("FireAndForget")]
//     public async Task<ActionResult> FireAndForget()
//     {
//       _logger.Info($"New job enqueue request received at {DateTime.Now}");
//       BackgroundJob.Enqueue(() => _jobRunService.LongRunningTask());
//       _logger.Info($"Response sent back at {DateTime.Now}");
//       return Ok();
//     }

//     [HttpPost("DelayStart")]
//     public async Task<ActionResult> DelayStart()
//     {
//       _logger.Info($"New job enqueue request received at {DateTime.Now}");
//       var jobId = BackgroundJob.Schedule(() => _jobRunService.LongRunningTask(), new TimeSpan(0, 0, 0, 60));
//       _logger.Info($"Response for JobId: {jobId} sent back at {DateTime.Now}");
//       return Ok();
//     }

//     [HttpPost("RecurringJob")]
//     public async Task<ActionResult> RepeatJob()
//     {
//       _logger.Info($"New job enqueue request received at {DateTime.Now}");
//       RecurringJob.AddOrUpdate("Recurring job", () => _jobRunService.LongRunningTask(), "*/1 * * * *");
//       _logger.Info($"Response for Reccuring Job sent back at {DateTime.Now}");
//       return Ok();
//     }

//     [HttpPost("DependentJob")]
//     public async Task<ActionResult> ChildJob()
//     {
//       _logger.Info($"New job enqueue request received at {DateTime.Now}");
//       var jobId = BackgroundJob.Schedule(() => _jobRunService.LongRunningTask(), new TimeSpan(0, 0, 0, 10));
//       BackgroundJob.ContinueJobWith(jobId, () => _jobRunService.LongRunningTask());
//       _logger.Info($"Response for dependent Job sent back at {DateTime.Now}");
//       return Ok();
//     }

//     /// <summary>
//     /// Trigger task deadline notification job manually for testing
//     /// </summary>
//     /// <returns>Result of job execution</returns>
//     [HttpPost("trigger-task-notifications")]
//     public async Task<ActionResult> TriggerTaskNotifications()
//     {
//       try
//       {
//         _logger.Info($"[Manual Trigger] Task notification job triggered at {DateTime.Now}");

//         // Execute job immediately
//         await _notificationService.CheckAndUpdateTaskNotificationsAsync();

//         return Ok(new
//         {
//           success = true,
//           message = "Task notification job executed successfully",
//           executedAt = DateTime.Now
//         });
//       }
//       catch (Exception ex)
//       {
//         _logger.Error($"[Manual Trigger] Failed to execute task notification job: {ex.Message}", ex);
//         return StatusCode(500, new
//         {
//           success = false,
//           message = "Failed to execute task notification job",
//           error = ex.Message
//         });
//       }
//     }

//     /// <summary>
//     /// Get information about registered recurring jobs
//     /// </summary>
//     /// <returns>List of recurring jobs</returns>
//     [HttpGet("recurring-jobs")]
//     public ActionResult GetRecurringJobs()
//     {
//       try
//       {
//         var connection = JobStorage.Current.GetConnection();
//         var recurringJobs = connection.GetRecurringJobs();

//         var jobInfo = recurringJobs.Select(job => new
//         {
//           id = job.Id,
//           cron = job.Cron,
//           nextExecution = job.NextExecution,
//           lastExecution = job.LastExecution,
//           lastJobId = job.LastJobId,
//           createdAt = job.CreatedAt
//         });

//         return Ok(new
//         {
//           success = true,
//           totalJobs = recurringJobs.Count,
//           jobs = jobInfo
//         });
//       }
//       catch (Exception ex)
//       {
//         _logger.Error($"Failed to get recurring jobs: {ex.Message}", ex);
//         return StatusCode(500, new
//         {
//           success = false,
//           message = "Failed to retrieve recurring jobs",
//           error = ex.Message
//         });
//       }
//     }
//   }
// }

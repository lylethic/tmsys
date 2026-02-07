using System;
using Hangfire;
using server.Common.Interfaces;
using server.Services;

namespace server.Common.Repository;

public class RecurringJobSetupService : IRecurringJobSetup
{
    private readonly NotificationService _service;
    private readonly ILogManager _logManager;

    public RecurringJobSetupService(
        IServiceProvider serviceProvider,
        ILogManager logManager)
    {
        _service = serviceProvider.GetRequiredService<NotificationService>();
        _logManager = logManager;
    }

    public void RegisterJobs(IRecurringJobManager recurringJobs)
    {
        // Job kiểm tra tasks đến hạn/quá hạn và gửi thông báo
        // Chạy mỗi 30 phút để đảm bảo thông báo kịp thời
        // recurringJobs.AddOrUpdate<NotificationService>(
        //     "TaskUpdateNotificationJob",
        //     service => service.CheckAndUpdateTaskNotificationsAsync(),
        //     Cron.MinuteInterval(30) // Chạy mỗi 30 phút
        // );

        // _logManager.Info("[Hangfire] TaskUpdateNotificationJob has been registered successfully!");
        // _logManager.Info("[Hangfire] Job will run every 30 minutes to check task deadlines.");
    }
}


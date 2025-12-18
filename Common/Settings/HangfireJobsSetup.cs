using System;
using Hangfire;
using server.Services;

namespace server.Common.Settings;

public static class HangfireJobsSetup
{
    public static void Configure(IRecurringJobManager recurringJobs)
    {
        // recurringJobs.AddOrUpdate(
        //     "process-notifications",
        //     () => service.ProcessPendingNotificationsAsync(),
        //     Cron.Daily
        // );
    }
}

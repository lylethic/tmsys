using System;

namespace server.Notifications;

public class JobRunService : IJobRunService
{
    public async Task LongRunningTask()
    {
        await Task.Delay(5000);
        Console.WriteLine($"Long running task executed at {DateTime.Now}");
    }
}

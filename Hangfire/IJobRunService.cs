using System;

namespace server.Notifications;

public interface IJobRunService
{
    Task LongRunningTask();
}

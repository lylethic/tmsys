using System;
using Hangfire;

namespace server.Common.Interfaces;

public interface IRecurringJobSetup
{
    void RegisterJobs(IRecurringJobManager recurringJobs);
}

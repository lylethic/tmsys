using System;
using Microsoft.Extensions.Configuration;
using server.Common.Interfaces;
using server.Services;

namespace server.Hubs;

/// <summary>
/// Worker is a background service that runs automatically and continuously.
/// </summary>
public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _notificationCheckInterval;

    public Worker(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        var delayMinutes = configuration.GetValue<int>("WorkerSettings:NotificationCheckIntervalMinutes", 1440);
        _notificationCheckInterval = TimeSpan.FromMinutes(Math.Max(delayMinutes, 60));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     using var scope = _serviceProvider.CreateScope();
        //     var logger = scope.ServiceProvider.GetRequiredService<ILogManager>();
        //     var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        //     try
        //     {
        //         logger.Info("Worker running at: " + DateTimeOffset.Now);
        //         // await notificationService.CheckAndUpdateTaskNotificationsAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.Error(ex.Message);
        //     }
        //     finally
        //     {
        //         logger.Info($"Worker will sleep for {_notificationCheckInterval.TotalMinutes} minutes.");
        //         await Task.Delay(_notificationCheckInterval, stoppingToken);
        //     }
        // }
    }
}

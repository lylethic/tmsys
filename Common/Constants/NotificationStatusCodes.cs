using System;
using System.Collections.Generic;
using server.Domain.Entities;

namespace server.Common.Constants;

public static class NotificationStatusCodes
{
    public static class Status
    {
        public static readonly Guid PendingNotification = Guid.Parse("b36a9d19-5c46-4208-8d9e-6f0450e4f0ca");
        public static readonly Guid SentNotification = Guid.Parse("d9fb4ad4-f470-4e2f-8e9f-8a1d8a99c0a1");
        public static readonly Guid FailedNotification = Guid.Parse("1f89fed2-c446-4461-9c70-40b0e399ff64");
        public static readonly Guid ReadNotification = Guid.Parse("5b9d8d66-4143-4b56-834c-2b7df3dad68c");
    }

    /// <summary>
    /// Provides seed records that can be inserted into <c>approved_status</c> table to guarantee existing codes.
    /// </summary>
    public static IEnumerable<Approved_status> SeedStatuses()
    {
        return new[]
        {
            new Approved_status
            {
                id = Status.PendingNotification,
                name = "Pending",
                code = "NOTI_PENDING",
                color = "#f0ad4e",
                bgcolor = "#fff5e6",
                type = "notification",
                sort_order = 10
            },
            new Approved_status
            {
                id = Status.SentNotification,
                name = "Sent",
                code = "NOTI_SENT",
                color = "#5cb85c",
                bgcolor = "#e8f5e8",
                type = "notification",
                sort_order = 20
            },
            new Approved_status
            {
                id = Status.ReadNotification,
                name = "Read",
                code = "NOTI_READ",
                color = "#0275d8",
                bgcolor = "#e7f0ff",
                type = "notification",
                sort_order = 30
            },
            new Approved_status
            {
                id = Status.FailedNotification,
                name = "Failed",
                code = "NOTI_FAILED",
                color = "#d9534f",
                bgcolor = "#fdecea",
                type = "notification",
                sort_order = 40
            }
        };
    }
}

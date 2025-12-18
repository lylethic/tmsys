using System;

namespace server.Hubs;

public interface INotificationHub
{
    Task ReceiveMessage(string user, string message);
}

using System;
using Microsoft.AspNetCore.SignalR;
using server.Services;

namespace server.Hubs;

public class NotificationHub(IAssistantService assistantService) : Hub<INotificationHub>
{
    private readonly IAssistantService _assistantService = assistantService;

    /// <summary>
    /// Send message to all clients
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task BroadcastMessage(string message)
    {
        // "System" can be used to indicate a server-wide broadcast
        await Clients.All.ReceiveMessage("System", message);
    }

    /// <summary>
    /// Send a message to a specific user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendToUser(string userId, string message)
    {
        await Clients.Group(userId).ReceiveMessage(userId, message);
    }

    /// <summary>
    /// Chat to another User 
    /// </summary>
    /// <param name="targetUserId">ID of user.</param>
    /// <param name="message">Message</param>
    public async Task SendMessageToUser(string targetUserId, string message)
    {
        var senderUserId = _assistantService.UserId;
        // Send message to the target user
        await Clients.Group(targetUserId).ReceiveMessage(senderUserId, message);
        // Gửi lại tin nhắn cho chính người gửi để cập nhật UI
        await Clients.Client(Context.ConnectionId).ReceiveMessage(senderUserId, message);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _assistantService.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }
}



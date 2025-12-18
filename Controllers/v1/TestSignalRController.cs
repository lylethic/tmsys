using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Hubs;

namespace server.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/test-signalR")]
    public class TestSignalRController : BaseApiController
    {
        private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;
        public TestSignalRController(IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogManager logger, IHubContext<NotificationHub, INotificationHub> hubContext) : base(mapper, httpContextAccessor, logger)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastMessage([FromQuery] string message)
        {
            await _hubContext.Clients.All.ReceiveMessage("API_Test", message);
            return Ok($"Message '{message}' broadcasted to all users.");
        }

        /// <summary>
        /// Sends a message to a specific user.
        /// </summary>
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromQuery] string userId, [FromQuery] string message)
        {
            // Note: This sends to a group named after the userId.
            // The user must be connected for this to work.
            await _hubContext.Clients.Group(userId).ReceiveMessage(userId, message);
            return Ok($"Message '{message}' sent to user '{userId}'.");
        }
    }
}

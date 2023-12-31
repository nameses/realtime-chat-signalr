﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using webapi.DTO;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly MessageSaverService _messageSaver;

        public ChatController(IHubContext<ChatHub> hubContext, MessageSaverService messageSaver)
        {
            _hubContext = hubContext;
            _messageSaver=messageSaver;
        }

        [Route("send")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendRequest([FromBody] ChatMessage msg)
        {
            await _messageSaver.AddMessageAsync(msg);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", msg.user, msg.msgText);
            return Ok();
        }

        [Route("send/private")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendRequest([FromBody] PrivateChatMessage msg)
        {
            //await _messageSaver.AddMessageAsync(msg);
            await _hubContext.Clients.Client(msg.receiverConnectionId).SendAsync("ReceivePrivateMessage", msg.user, msg.msgText, msg.receiverUsername);
            return Ok();
        }
    }
}
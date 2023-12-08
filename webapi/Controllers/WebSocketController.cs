using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace webapi.Controllers
{
    [Route("")]
    public class WebSocketController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        //[Route("/ws")]
        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    if (HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //        //await HandleWebSocketConnection(webSocket);
        //        return Ok();
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, webSocket);

            await SendConnectedMessage(connectionId);

            await Echo(webSocket);
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await HandleReceivedMessage(receiveResult, buffer);
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            string connectionIdToRemove = _sockets.FirstOrDefault(x => x.Value == webSocket).Key;
            _sockets.TryRemove(connectionIdToRemove, out _);
            await SendDisconnectedMessage(connectionIdToRemove);
        }

        private async Task HandleReceivedMessage(WebSocketReceiveResult result, byte[] buffer)
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await HandleTextMessage(message);
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                await HandleBinaryMessage(buffer, result.Count);
            }
        }

        private async Task SendConnectedMessage(string username)
        {
            string message = $"User {username} connected.";
            await BroadcastMessage(message);
        }

        private async Task SendDisconnectedMessage(string username)
        {
            string message = $"User {username} disconnected.";
            await BroadcastMessage(message);
        }

        private async Task HandleTextMessage(string message)
        {
            await BroadcastMessage($"Received text message: {message}");
        }

        private async Task HandleBinaryMessage(byte[] buffer, int count)
        {
            await BroadcastImage("SenderConnectionId", buffer, count);
        }

        private async Task BroadcastImage(string senderConnectionId, byte[] buffer, int count)
        {
            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open && socket != _sockets[senderConnectionId])
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
        }

        private async Task BroadcastMessage(string message)
        {
            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}

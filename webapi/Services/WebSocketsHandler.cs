using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace webapi.Services
{
    public class WebSocketsHandler
    {
        private readonly ConcurrentDictionary<string, UserWebSocket> _sockets = new ConcurrentDictionary<string, UserWebSocket>();

        public async Task HandleWebSocketConnection(WebSocket webSocket, string username)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, new UserWebSocket { Username = username, WebSocket = webSocket });

            //await SendConnectedMessage(connectionId);

            await Echo(webSocket);
        }

        public async Task Echo(WebSocket webSocket)
        {
            var maxBufferSize = 1024 * 1024 * 64;

            while (true)
            {
                var buffer = new byte[maxBufferSize];
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.CloseStatus.HasValue)
                {
                    await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
                    string connectionIdToRemove = _sockets.FirstOrDefault(x => x.Value.WebSocket == webSocket).Key;
                    _sockets.TryRemove(connectionIdToRemove, out _);
                    //await SendDisconnectedMessage(connectionIdToRemove);
                    break;
                }

                await HandleReceivedMessage(receiveResult, buffer);
            }
        }

        //public async Task Echo(WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 1024 * 64];
        //    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //    while (!receiveResult.CloseStatus.HasValue)
        //    {
        //        await HandleReceivedMessage(receiveResult, buffer);
        //        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }

        //    await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        //    string connectionIdToRemove = _sockets.FirstOrDefault(x => x.Value.WebSocket == webSocket).Key;
        //    _sockets.TryRemove(connectionIdToRemove, out _);
        //    //await SendDisconnectedMessage(connectionIdToRemove);
        //}

        public async Task HandleReceivedMessage(WebSocketReceiveResult result, byte[] buffer)
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

        //public async Task SendConnectedMessage(string username)
        //{
        //    string message = $"User {username} connected.";
        //    await BroadcastMessage(message);
        //}

        //public async Task SendDisconnectedMessage(string username)
        //{
        //    string message = $"User {username} disconnected.";
        //    await BroadcastMessage(message);
        //}

        public async Task HandleTextMessage(string message)
        {
            await BroadcastMessage($"Received text message: {message}");
        }

        public async Task HandleBinaryMessage(byte[] buffer, int count)
        {
            await BroadcastImage(buffer, count);
        }

        //public async Task BroadcastImage(byte[] buffer, int count)
        //{
        //    foreach (var userSocket in _sockets.Values)
        //    {
        //        if (userSocket?.WebSocket!.State == WebSocketState.Open)
        //        {
        //            var message = JsonSerializer.Serialize(new { data = new ArraySegment<byte>(buffer, 0, count), username = userSocket.Username, msgType = (int)MsgType.Image });
        //            await userSocket.WebSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
        //        }
        //    }
        //}


        public async Task BroadcastImage(byte[] buffer, int count)
        {
            foreach (var userSocket in _sockets.Values)
            {
                if (userSocket?.WebSocket!.State == WebSocketState.Open)
                {
                    var base64Data = Convert.ToBase64String(buffer, 0, count);
                    var message = JsonSerializer.Serialize(new { data = base64Data, username = userSocket.Username, msgType = (int)MsgType.Image });
                    await userSocket.WebSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        public async Task BroadcastMessage(string msgTxt)
        {
            foreach (var userSocket in _sockets.Values)
            {
                if (userSocket?.WebSocket!.State == WebSocketState.Open)
                {
                    var message = JsonSerializer.Serialize(new { data = msgTxt, username = userSocket.Username, msgType = (int)MsgType.Text });
                    await userSocket.WebSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        public class UserWebSocket
        {
            public WebSocket? WebSocket;
            public string? Username;
        }
        public enum MsgType
        {
            Text = 0, Image = 1
        }
    }
}
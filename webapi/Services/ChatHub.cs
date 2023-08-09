using Microsoft.AspNetCore.SignalR;
using webapi.DTO;

namespace webapi.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        
        public Task SendToUser(string user, string receiverConnectionId, string message)
        {
            return Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", user, message);
        }

        public string GetConnectionId() => Context.ConnectionId;

        //public async Task BroadcastAsync(ChatMessage message)
        //{
        //    await Clients.All.MessageReceivedFromHub(message);
        //}

        //public override Task OnConnectedAsync()
        //{
        //    return Clients.All.NewUserConnected("New user connected");
        //}
    }
}
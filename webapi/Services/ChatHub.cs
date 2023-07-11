using Microsoft.AspNetCore.SignalR;
using webapi.DTO;

namespace webapi.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveOne", user, message);
        }

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
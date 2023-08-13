using Microsoft.AspNetCore.SignalR;
using webapi.DTO;

namespace webapi.Services
{
    public class ChatHub : Hub
    {
        private readonly IOnlineUserRepository _repository;
        public ChatHub(IOnlineUserRepository repository)
        {
            _repository = repository;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        
        public Task SendToUser(string user, string receiverConnectionId, string message)
        {
            return Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", user, message);
        }

        public string GetConnectionId() => Context.ConnectionId;

        public override async Task OnConnectedAsync()
        {
            string сonnectionId = Context.ConnectionId;
            string username = Context.GetHttpContext().Request.Query["username"];

            //add new user to current context
            if(username!=null && сonnectionId!=null)
                _repository.AddOrUpdate(username, сonnectionId);

            await Clients.All.SendAsync("NewUserConnected", username);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _repository.RemoveByConnectionId(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        //public override Task OnConnectedAsync()
        //{
        //    return Clients.All.NewUserConnected("New user connected");
        //}
        //public async Task BroadcastAsync(ChatMessage message)
        //{
        //    await Clients.All.MessageReceivedFromHub(message);
        //}
    }
}
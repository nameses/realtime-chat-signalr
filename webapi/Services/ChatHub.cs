using Microsoft.AspNetCore.SignalR;
using webapi.DTO;
using webapi.Entities;

namespace webapi.Services
{
    public class ChatHub : Hub
    {
        private readonly OnlineUserRepository _repository;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(OnlineUserRepository repository,ILogger<ChatHub> logger)
        {
            _repository = repository;
            _logger=logger;
        }

        public async Task SendMessage(string user, string message)
        {
            _logger.LogInformation($"Chathub - SendMessage(user:{user},message: {message})");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        
        public Task SendToUser(string user, string message, string receiverConnectionId, string receiverUsername)
        {
            _logger.LogInformation($"Chathub - SendToUser(user:{user},message: {message}, receiverUsername:{receiverUsername})");
            return Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", user, message, receiverUsername);
        }

        public string GetConnectionId() => Context.ConnectionId;

        public async Task OnConnectedWithUsername(string username)
        {
            _logger.LogInformation($"Chathub - OnConnectedWithUsername(username:{username})");
            string сonnectionId = Context.ConnectionId;
            //string username = Context.GetHttpContext().Request.Query["username"];

            //add new user to current context
            if(username!=null && сonnectionId!=null)
                await _repository.AddOrUpdateAsync(username, сonnectionId);

            await Clients.All.SendAsync("NewUserConnected", username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Chathub - OnDisconnectedAsync(connectionId:{Context.ConnectionId})");
            await _repository.RemoveByConnectionIdAsync(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
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
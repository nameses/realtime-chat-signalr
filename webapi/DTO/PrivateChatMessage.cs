namespace webapi.DTO
{
    public class PrivateChatMessage
    {
        public string user { get; set; }
        public string msgText { get; set; }
        public string? receiverConnectionId { get; set; }
        public string? receiverUsername { get; set; }
    }
}

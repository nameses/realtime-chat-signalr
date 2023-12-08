namespace webapi.DTO
{
    public class Message
    {
        public string Content { get; set; }
        public string Username { get; set; }
        public DateTime DateTime { get; set; }
    }

    public enum ChatMessageType
    {
        Text = 0,
        Photo = 1
    }
}
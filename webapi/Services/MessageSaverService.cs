using Microsoft.Extensions.Options;
using System.Text.Json;
using webapi.Configuration;
using webapi.DTO;

namespace webapi.Services
{
    public class MessageSaverService
    {
        private readonly string _filePath;
        private readonly IOptions<MessagesPath> _settings;

        public MessageSaverService(IOptions<MessagesPath> settings)
        {
            _settings=settings;

            string directoryPath = Path.Combine(_settings.Value.Path + "savedmessages");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            _filePath = Path.Combine(directoryPath + "/messages.json");
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            var json = JsonSerializer.Serialize(message) + Environment.NewLine;
            await File.AppendAllTextAsync(_filePath, json);
        }
    }
}

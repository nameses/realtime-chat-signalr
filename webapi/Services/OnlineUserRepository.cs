using StackExchange.Redis;
using webapi.DTO;

namespace webapi.Services
{
    public class OnlineUserRepository
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<OnlineUserRepository> _logger;

        private readonly string HASH_KEY = "onlineusers";

        public OnlineUserRepository(ILogger<OnlineUserRepository> logger)
        {
            _redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions()
                {
                    EndPoints = { "localhost:6379" },
                    AllowAdmin = true,
                    ConnectTimeout = 30000,
                    SyncTimeout = 30000
                });
            _db = _redis.GetDatabase();
            _logger=logger;
            if (_redis.IsConnected)
            {
                _logger.LogInformation("Connection established.");
                var pong = _db.Ping();
                _logger.LogInformation("Trying to ping database. Result: " + pong.ToString());
            }
            else _logger.LogInformation("Connection failed.");

            ClearUserConnections();
        }

        public async Task<List<UsernameConnectionMapping>> GetAllUsersAsync()
        {
            var hashEntries = await _db.HashGetAllAsync(HASH_KEY);
            return hashEntries
                .Select(pair => new UsernameConnectionMapping() { Username=pair.Name, ConnectionId=pair.Value })
                .ToList();
        }

        public async Task AddOrUpdateAsync(string username, string connectionId)
        {
            await _db.HashSetAsync(HASH_KEY, username, connectionId);
        }

        public async void RemoveByUsername(string username)
        {
            await _db.HashDeleteAsync(HASH_KEY, username);
        }

        public async Task RemoveByConnectionIdAsync(string connectionId)
        {
            var username = await GetUsernameByConnectionId(connectionId);

            if (!string.IsNullOrEmpty(username))
                await _db.HashDeleteAsync(HASH_KEY, username);
        }
        public async Task<string> GetUsernameByConnectionId(string connectionId)
        {
            //return _db.StringGet(connectionId);

            List<UsernameConnectionMapping> list = await GetAllUsersAsync();
            var foundUsername = list.FirstOrDefault(pair => pair.ConnectionId==connectionId)?.Username;

            return foundUsername!;
        }
        public async Task<string> GetConnectionIdAsync(string username)
        {
            var value = await _db.HashGetAsync(HASH_KEY, username);
            return value.ToString();
        }

        public async void ClearUserConnections()
        {
            var keys = await _db.HashKeysAsync(HASH_KEY);
            if (keys.Length > 0)
            {
                _db.HashDelete(HASH_KEY, keys);
                _logger.LogInformation($"Hash \'{HASH_KEY}\' is cleared from redis db");
            }
            else
            {
                _logger.LogInformation($"Hash '{HASH_KEY}' was empty in db");
            }
        }
    }

}

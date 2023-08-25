using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net.Http;
using webapi.DTO;
using webapi.Entities;
using webapi.Migrations;

namespace webapi.Services
{
    public class OnlineUserRepository
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<OnlineUserRepository> _logger;

        private readonly string USERNAME_KEY = "usernamekey:";

        //private const string OnlineUsersKey = "OnlineUsers";
        //private readonly List<UsernameConnectionMapping> _connections = new();
        public OnlineUserRepository(ILogger<OnlineUserRepository> logger)
        {
            _redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions() 
                { EndPoints = { "localhost:6379" },
                    ConnectTimeout = 30000,
                    SyncTimeout = 30000
                });
            _db = _redis.GetDatabase();
            _logger=logger;
            if (_redis.IsConnected)
            {
                var pong = _db.Ping();
                _logger.LogInformation(pong.ToString());
                _logger.LogInformation("Connection established."); 
            }
            else _logger.LogInformation("Connection failed.");
        }

        //public async Task<List<UsernameConnectionMapping>> GetOnlineUsers()
        //{
        //    var usersObject = await _distributedCache.GetStringAsync(OnlineUsersKey);

        //    if (string.IsNullOrWhiteSpace(usersObject))
        //    {
        //        _logger.LogInformation("Online users not found.");
        //        return null;
        //    }

        //    var usersList = JsonConvert.DeserializeObject<List<UsernameConnectionMapping>>(usersObject);

        //    return usersList;

        //    //var memoryCacheEntryOptions = new DistributedCacheEntryOptions
        //    //{
        //    //    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
        //    //    SlidingExpiration = TimeSpan.FromSeconds(1200)
        //    //};
        //    //await _distributedCache.SetStringAsync(CountriesKey, responseData, memoryCacheEntryOptions);
        //}
        public async Task<List<UsernameConnectionMapping>> GetAllUsersAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(database: _db.Database)
                            .Where(value => value.ToString().StartsWith(USERNAME_KEY))
                            .Select(key => key.ToString());

            var list = new List<UsernameConnectionMapping>();
            foreach (var key in keys)
            {
                var value = await _db.StringGetAsync(key);
                list.Add(new UsernameConnectionMapping() 
                { 
                    Username=key.Split(':')[1], 
                    ConnectionId=value.ToString() 
                });
            }

            return list;
        }

        public async Task AddOrUpdateAsync(string username, string connectionId)
        {
            await _db.StringSetAsync(USERNAME_KEY + username, connectionId);

            //_db.SetAdd("connectionids:index", connectionId);
        }

        public void RemoveByUsername(string username)
        {
            _db.KeyDelete(USERNAME_KEY + username);
        }

        public async Task RemoveByConnectionIdAsync(string connectionId)
        {
            var username = await GetUsernameByConnectionId(connectionId);

            if (!string.IsNullOrEmpty(username))
            {
                // Remove the key-value pair using the username
                _db.KeyDelete(USERNAME_KEY + username);
                try
                {
                    _db.StringGet(USERNAME_KEY + username);
                }
                catch (Exception ex) 
                { 
                    _logger.LogInformation($"Key for username: {username} still in db."); 
                }

                // Clean up the secondary index set
                //_db.SetRemove("connectionids:index", connectionId);
            }
        }
        public async Task<string> GetUsernameByConnectionId(string connectionId)
        {
            //return _db.StringGet(connectionId);

            var list = await GetAllUsersAsync();
            var foundUsername = list.FirstOrDefault(pair => pair.ConnectionId==connectionId)?.Username;

            return foundUsername!;
        }
        public string GetConnectionId(string username)
        {
            return _db.StringGet(USERNAME_KEY + username).ToString();
        }
    }
    
}

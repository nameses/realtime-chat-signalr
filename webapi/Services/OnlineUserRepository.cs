using webapi.DTO;

namespace webapi.Services
{
    public interface IOnlineUserRepository
    {
        void AddOrUpdate(string username, string connectionId);
        public void RemoveByUsername(string username);
        public void RemoveByConnectionId(string connectionId);
        string GetConnectionId(string username);
        List<UsernameConnectionMapping> GetAllUsernames();
    }
    public class OnlineUserRepository : IOnlineUserRepository
    {
        private readonly List<UsernameConnectionMapping> _connections = new();

        public void AddOrUpdate(string username, string connectionId)
        {
            var existingMapping = _connections.FirstOrDefault(x => x.Username == username);
            if (existingMapping != null)
            {
                existingMapping.ConnectionId = connectionId;
            }
            else
            {
                _connections.Add(new UsernameConnectionMapping { Username = username, ConnectionId = connectionId });
            }
        }

        public void RemoveByUsername(string username)
        {
            var mapping = _connections.FirstOrDefault(x => x.Username == username);
            if (mapping != null)
            {
                _connections.Remove(mapping);
            }
        }
        public void RemoveByConnectionId(string connectionId)
        {
            var mapping = _connections.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (mapping != null)
            {
                _connections.Remove(mapping);
            }
        }

        public string GetConnectionId(string username)
        {
            var mapping = _connections.FirstOrDefault(x => x.Username == username);
            return mapping?.ConnectionId;
        }

        public List<UsernameConnectionMapping> GetAllUsernames()
        {
            return _connections.ToList();
        }
    }
    
}

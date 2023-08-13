using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Context;
using webapi.DTO;
using webapi.Entities;
using webapi.Helper;

namespace webapi.Services
{
    public interface IUserService
    {
        Task<User?> GetByUsername(string username);
        Task<User?> GetById(int id);
        Task<List<UsernameConnectionMapping>?> GetConnectedUsers();
        Task<int?> CreateAsync(User user);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IOnlineUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly Encrypter _encrypter;

        public UserService(AppDbContext context,IOnlineUserRepository repository, ILogger<UserService> logger, Encrypter encrypter)
        {
            _context=context;
            _repository=repository;
            _logger=logger;
            _encrypter=encrypter;
        }
        public async Task<User?> GetByUsername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user==null)
            {
                _logger.LogInformation($"User({username}) not found in DB");
                return null;
            }

            _logger.LogInformation($"User(username={username},id={user.Id}) successfully found in DB");
            return user;
        }
        public async Task<User?> GetById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user==null)
            {
                _logger.LogInformation($"User(id={id}) not found in DB");
                return null;
            }

            _logger.LogInformation($"User(id={id}) successfully found in DB");
            return user;
        }

        public async Task<List<UsernameConnectionMapping>?> GetConnectedUsers()
        {
            var users = _repository.GetAllUsernames();

            if (users==null || users.Count==0)
            {
                _logger.LogInformation($"Connected users not found ");
                return null;
            }

            _logger.LogInformation($"Connected users successfully found ");
            return users;
        }

        public async Task<int?> CreateAsync(User user)
        {
            if (user==null) 
            {
                _logger.LogInformation($"User for creation is not valid");
                throw new ArgumentNullException("User for creation is not valid"); 
            }

            user.Password = _encrypter.Encrypt(user.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created user with ID {user.Id}");

            return user.Id;
        }
    }
}

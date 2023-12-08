using Microsoft.EntityFrameworkCore;
using webapi.Context;
using webapi.Entities;
using webapi.Helper;
using webapi.Models;

namespace webapi.Services
{
    public interface IUserService
    {
        Task<User?> GetByUsername(string username);
        Task<User?> GetById(int id);
        //Task<List<UsernameConnectionMapping>?> GetConnectedUsers();
        Task<User?> CreateAsync(UserModel user);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly Encrypter _encrypter;

        public UserService(AppDbContext context, ILogger<UserService> logger, Encrypter encrypter)
        {
            _context=context;
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

            return user;
        }

        public async Task<User?> CreateAsync(UserModel user)
        {
            if (user==null)
            {
                _logger.LogInformation($"User for creation is not valid");
                throw new ArgumentNullException("User for creation is not valid");
            }
            if (user.Password==null) return null;
            user.Password = _encrypter.Encrypt(user.Password);

            var createdEntity = await _context.Users.AddAsync(new User { Username=user.Username, Password=user.Password });
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created user with ID {createdEntity.Entity.Id}");

            return createdEntity.Entity;
        }
    }
}

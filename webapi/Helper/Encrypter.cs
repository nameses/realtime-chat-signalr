using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using webapi.Configuration;

namespace webapi.Helper
{
    public class Encrypter
    {
        private readonly IOptions<PasswordEncryption> _settings;

        public Encrypter(IOptions<PasswordEncryption> settings)
        {
            _settings=settings;
        }

        public string Encrypt(string password)
        {
            string salt = _settings.Value.Key;
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }
    }
}

using System.Security.Cryptography;
using System.Text;
namespace TradePlatform.Api.Services
{
    public interface IPasswordHasher
    {
        byte[] HashPassword(string password);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public byte[] HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public class PasswordHashingService
    {
        private readonly IPasswordHasher _hasher;

        public PasswordHashingService(IPasswordHasher hasher)
        {
            _hasher = hasher;
        }

        public string HashToBase64(string password)
        {
            var hashedBytes = _hasher.HashPassword(password);
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

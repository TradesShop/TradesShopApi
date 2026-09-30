using System.Security.Cryptography;
using System.Text;

namespace TradePlatform.Api.Services;

public class PasswordHasher : IPasswordHasher
{
	public byte[] HashPassword(string password)
	{
		using SHA256 sha = SHA256.Create();
		return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
	}
}

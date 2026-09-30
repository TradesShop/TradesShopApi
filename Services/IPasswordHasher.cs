namespace TradePlatform.Api.Services;

public interface IPasswordHasher
{
	byte[] HashPassword(string password);
}

using System;

namespace TradePlatform.Api.Services;

public class PasswordHashingService
{
	private readonly IPasswordHasher _hasher;

	public PasswordHashingService(IPasswordHasher hasher)
	{
		_hasher = hasher;
	}

	public string HashToBase64(string password)
	{
		byte[] hashedBytes = _hasher.HashPassword(password);
		return Convert.ToBase64String(hashedBytes);
	}
}

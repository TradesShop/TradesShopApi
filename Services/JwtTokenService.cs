using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services;

public class JwtTokenService : IJwtTokenService
{
	private readonly IConfiguration _config;

	private readonly JwtSettings _settings;

	public JwtTokenService(IConfiguration config, IOptions<JwtSettings> settings)
	{
		_config = config;
		_settings = settings.Value;
	}

	public string GenerateToken(User user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		string key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
		string issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing");
		string audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing");
		List<Claim> claims = new List<Claim>
		{
			new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.id.ToString()),
			new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", user.user_type.ToString()),
			new Claim("email", user.email ?? string.Empty),
			new Claim("country", user.country_code ?? "UK")
		};
		SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		SigningCredentials creds = new SigningCredentials(securityKey, "HS256");
		DateTime? expires = DateTime.UtcNow.AddHours(12.0);
		SigningCredentials signingCredentials = creds;
		JwtSecurityToken token = new JwtSecurityToken(issuer, audience, claims, null, expires, signingCredentials);
		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		byte[] randomNumber = new byte[64];
		using RandomNumberGenerator rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}
}

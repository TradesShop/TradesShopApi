using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services;

public class AuthService : IAuthService
{
	private readonly IUsersRepository _users;

	private readonly PasswordHashingService _passwordHashing;

	private readonly IUserAddressRepository _addresses;

	private readonly ITradespersonsRepository _tradespersons;

	private readonly IPasswordHasher _hasher;

	private readonly IJwtTokenService _jwtService;

	private readonly IRefreshTokenRepository _refreshtoken;

	public AuthService(IUsersRepository users, IUserAddressRepository addresses, PasswordHashingService passwordHashing, ITradespersonsRepository tradespersons, IPasswordHasher hasher, IJwtTokenService jwt, IRefreshTokenRepository refreshtoken)
	{
		_users = users;
		_addresses = addresses;
		_passwordHashing = passwordHashing;
		_tradespersons = tradespersons;
		_hasher = hasher;
		_jwtService = jwt;
		_refreshtoken = refreshtoken;
	}

	private async Task<(string token, string refresh_token)> GenerateAuthTokensAsync(User user)
	{
		string token = _jwtService.GenerateToken(user);
		string refresh_token = _jwtService.GenerateRefreshToken();
		RefreshToken refreshEntity = new RefreshToken
		{
			user_id = user.id,
			token = refresh_token,
			expires_at = DateTime.UtcNow.AddDays(10.0),
			isrevoked = false,
			isused = false
		};
		await _refreshtoken.AddAsync(refreshEntity);
		return (token: token, refresh_token: refresh_token);
	}

	private async Task<RegisterResponse> BuildAuthResponse(User? user, string action)
	{
		if (user == null)
		{
			return new RegisterResponse
			{
				token = null,
				refresh_token = null,
				User = null,
				message = ((action == "login") ? "Invalid email or password" : "Registration failed")
			};
		}
		var (accessToken, refreshToken) = await GenerateAuthTokensAsync(user);
		return new RegisterResponse
		{
			token = accessToken,
			refresh_token = refreshToken,
			User = new { user.id, user.email, user.user_type, user.phone, user.firstname, user.lastname },
			message = ((action == "login") ? "Login successful" : "Registration successful")
		};
	}

	public async Task<RegisterResponse> CreateAuthTokensAsync(User user)
	{
		return await BuildAuthResponse(user, "login");
	}

	public async Task<RefreshResult> RefreshTokensAsync(string refreshToken)
	{
		RefreshToken stored = await _refreshtoken.GetByTokenAsync(refreshToken);
		if (stored == null || stored.isrevoked || stored.isused || stored.expires_at <= DateTime.UtcNow)
		{
			return RefreshResult.Fail();
		}
		User user = await _users.GetByIdAsync(stored.user_id);
		if (user == null)
		{
			return RefreshResult.Fail();
		}
		(string, string) tuple = await GenerateAuthTokensAsync(user);
		string token = tuple.Item1;
		string refresh_token = tuple.Item2;
		stored.isused = true;
		stored.isrevoked = true;
		await _refreshtoken.UpdateAsync(stored);
		return RefreshResult.Ok(token, refresh_token);
	}

	public async Task<RegisterResponse> UserUpsertAsync(RegisterDto reg_dto)
	{
		UserDto user = new UserDto
		{
			firstname = reg_dto.firstname,
			lastname = reg_dto.lastname,
			email = reg_dto.email,
			password_hash = _passwordHashing.HashToBase64(reg_dto.password_hash),
			phone = reg_dto.phone,
			user_type = reg_dto.user_type,
			country_code = reg_dto.country_code
		};
		User anyuser = await _users.UpdateAnyUserAsync(user);
		anyuser.verified = true;
		reg_dto.user_id = anyuser.id;
		if (reg_dto.user_type == 1)
		{
			User user2 = anyuser;
			user2.customer_id = await _addresses.CreateCustomerProfileAsync(reg_dto);
		}
		else if (reg_dto.user_type == 2)
		{
			User user2 = anyuser;
			user2.business_id = await _addresses.CreateTradeUserBusinessAsync(reg_dto);
		}
		return await BuildAuthResponse(anyuser, "register");
	}

	public async Task<RegisterResponse> LoginAsync(LoginDto dto)
	{
		string hashed = _passwordHashing.HashToBase64(dto.password);
		return await BuildAuthResponse(await _users.LoginAsync(dto.email, hashed), "login");
	}

	public async Task<bool> RevokeRefreshTokenByToken(RefreshToken anyRefreshToken)
	{
		await _refreshtoken.RevokeRefreshTokenByToken(anyRefreshToken);
		return true;
	}
}

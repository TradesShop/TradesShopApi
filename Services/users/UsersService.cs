using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.DTOs.Unsubscribe;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.MagicPayLoad;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.AESHelper;

namespace TradePlatform.Api.Services.users;

public class UsersService : IUsersService
{
	private readonly IUsersRepository _usersRepo;

	private readonly PasswordHashingService _passwordHashing;

	private readonly IAesCryptoService _aes;

	public UsersService(IUsersRepository usersRepo, IAesCryptoService aes, PasswordHashingService passwordHashing)
	{
		_usersRepo = usersRepo;
		_passwordHashing = passwordHashing;
		_aes = aes;
	}

	public async Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
	{
		string oldHash = _passwordHashing.HashToBase64(oldPassword);
		string newHash = _passwordHashing.HashToBase64(newPassword);
		return await _usersRepo.ChangePasswordAsync(userId, oldHash, newHash);
	}

	public async Task<AccountContextDto> UserAccountGetAsync(Guid user_id)
	{
		return await _usersRepo.UserAccountGetAsync(user_id);
	}

	public async Task<User> GetUserByIdAsync(Guid user_id)
	{
		return await _usersRepo.GetUserByIdAsync(user_id);
	}

	public async Task<UserBasic> GetUserBasicInfoById(Guid user_id)
	{
		return await _usersRepo.GetUserBasicInfoById(user_id);
	}

	public async Task<User> UpdateAnyUserAsync(UserDto uDto)
	{
		return await _usersRepo.UpdateAnyUserAsync(uDto);
	}

	public async Task<TradesPeopleResultDto> GetTradesPeopleListAsync(TradesPeopleSearchDto tps_dto)
	{
		return await _usersRepo.GetTradesPeopleListAsync(tps_dto);
	}

	public async Task<UserNotificationPreferences> UserNotificationPreferencesUpsert(UserNotificationPrefUpd unpUpd)
	{
		return await _usersRepo.UserNotificationPreferencesUpsert(unpUpd);
	}

	public async Task<UserNotificationPreferences> UserNotificationPreferencesForUser(UserNotificationPrefReq unpReq)
	{
		if (!string.IsNullOrWhiteSpace(unpReq.token))
		{
			string encrypted = Uri.UnescapeDataString(unpReq.token);
			string json;
			try
			{
				json = _aes.Decrypt(encrypted);
			}
			catch
			{
				return new UserNotificationPreferences
				{
					success = false,
					message = "Invalid Token"
				};
			}
			MagicPayload payload;
			try
			{
				payload = JsonConvert.DeserializeObject<MagicPayload>(json);
			}
			catch
			{
				return new UserNotificationPreferences
				{
					success = false,
					message = "Invalid decryption"
				};
			}
			if (payload.user_id != unpReq.user_id)
			{
				return new UserNotificationPreferences
				{
					success = false,
					message = "Invalid User"
				};
			}
		}
		return await _usersRepo.UserNotificationPreferencesForUser(unpReq);
	}

	public async Task user_event_insert_async(user_event anyevent)
	{
		await _usersRepo.user_event_insert_async(anyevent);
	}
}

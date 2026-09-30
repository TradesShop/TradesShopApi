using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.DTOs.Unsubscribe;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IUsersRepository
{
	Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPasswordHash, string newPasswordHash);

	Task<User> LoginAsync(string email, string passwordhash);

	Task RevokeRefreshTokenAsync(string token);

	Task<User> GetByEmailAsync(string email, int? usertype);

	Task<User> GetByIdAsync(Guid id);

	Task<UserBasic> GetUserBasicInfoById(Guid user_id);

	Task<User> GetTradeUserByIdAsync(Guid userid);

	Task<User> UpdateAnyUserAsync(UserDto user);

	Task UpdateStripeCustomerIdAsync(Guid userid, string stripeCustomerId);

	Task<AccountContextDto?> UserAccountGetAsync(Guid user_id);

	Task<User> GetUserByIdAsync(Guid user_id);

	Task<TradesPeopleResultDto> GetTradesPeopleListAsync(TradesPeopleSearchDto tps_dto);

	Task<UserNotificationPreferences> UserNotificationPreferencesForUser(UserNotificationPrefReq unpReq);

	Task<UserNotificationPreferences> UserNotificationPreferencesUpsert(UserNotificationPrefUpd unpUpd);

	Task user_event_insert_async(user_event anyevent);
}

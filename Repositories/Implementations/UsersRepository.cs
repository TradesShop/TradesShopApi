using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.DTOs.Unsubscribe;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class UsersRepository : IUsersRepository
{
	private readonly DapperContext _context;

	public UsersRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPasswordHash, string newPasswordHash)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			user_id = userId,
			old_password_hash = oldPasswordHash,
			new_password_hash = newPasswordHash
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstAsync<CommonResponseDto>("usp_user_change_password", param, null, null, commandType);
	}

	public async Task<AccountContextDto?> UserAccountGetAsync(Guid user_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<AccountContextDto>("usp_user_account_context_get_async", param, null, null, commandType);
	}

	public async Task RevokeRefreshTokenAsync(string token)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		await connection.ExecuteAsync("usp_RefreshTokens_RevokeByToken", new { token });
	}

	public async Task<User?> LoginAsync(string email, string passwordhash)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { email, passwordhash };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<User>("usp_users_login", param, null, null, commandType);
	}

	public async Task<User> GetByEmailAsync(string email, int? usertype)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			Email = email,
			UserType = usertype
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<User>("usp_users_get_by_email", param, null, null, commandType);
	}

	public async Task<User> GetByIdAsync(Guid id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		return await conn.QueryFirstOrDefaultAsync<User>("SELECT * FROM users WHERE id = @Id", new
		{
			Id = id
		});
	}

	public async Task<User> UpdateAnyUserAsync(UserDto user)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			firstname = user.firstname,
			lastname = user.lastname,
			email = user.email,
			password_hash = user.password_hash,
			phone = user.phone,
			user_type = user.user_type.Value,
			country_code = user.country_code
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<User>("[dbo].[usp_user_upsert]", param, null, null, commandType);
	}

	public async Task<User> GetTradeUserByIdAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<User>("usp_users_get_by_id_async", param, null, null, commandType);
	}

	public async Task<string?> GetStripeCustomerIdAsync(Guid userid)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = userid
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<string>("dbo.GetStripeCustomerId", param, null, null, commandType);
	}

	public async Task UpdateStripeCustomerIdAsync(Guid userid, string stripeCustomerId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = userid,
			stripe_customer_id = stripeCustomerId
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("dbo.usp_user_stripe_customerid_update", param, null, null, commandType);
	}

	public async Task<User> GetUserByIdAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<User>("usp_users_get_by_id_async", param, null, null, commandType);
	}

	public async Task<UserBasic> GetUserBasicInfoById(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = user_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<UserBasic>("[dbo].[usp_user_basic_get_by_id]", param, null, null, commandType);
	}

	public async Task<TradesPeopleResultDto> GetTradesPeopleListAsync(TradesPeopleSearchDto tps_dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { tps_dto.search, tps_dto.sort_by, tps_dto.page_number, tps_dto.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader result = await conn.QueryMultipleAsync("usp_users_list_for_admin", param, null, null, commandType);
		TradesPeopleResultDto traderslist = new TradesPeopleResultDto();
		traderslist.traders = result.Read<TradeUsersDto>().ToList();
		traderslist.total_records = result.ReadSingle<int>();
		return traderslist;
	}

	public async Task<UserNotificationPreferences> UserNotificationPreferencesForUser(UserNotificationPrefReq unpReq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { unpReq.user_id, unpReq.email };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<UserNotificationPreferences>("usp_user_notification_preferences_req", param, null, null, commandType);
	}

	public async Task<UserNotificationPreferences> UserNotificationPreferencesUpsert(UserNotificationPrefUpd unpUpd)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = unpUpd.user_id,
			email = unpUpd.email,
			receive_important = unpUpd.receive_important,
			receive_promotional = unpUpd.receive_promotional,
			return_preferences = true
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<UserNotificationPreferences>("usp_user_notification_preferences_upsert", param, null, null, commandType);
	}

	public async Task user_event_insert_async(user_event anyevent)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_user_event_insert]", anyevent, null, null, commandType);
	}
}

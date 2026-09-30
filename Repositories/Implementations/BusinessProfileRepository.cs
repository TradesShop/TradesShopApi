using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class BusinessProfileRepository : IBusinessProfileRepository
{
	private readonly DapperContext _context;

	public BusinessProfileRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<BusinessProfile?> GetByUserIdAsync(Guid userId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		return await conn.QueryFirstOrDefaultAsync<BusinessProfile>("\r\n                SELECT TOP 1 *\r\n                FROM [dbo].[business_profile]\r\n                WHERE user_id = @user_id\r\n                ORDER BY created_at DESC", new
		{
			user_id = userId
		});
	}

	public async Task<IntroMessageUpdateReqDto> business_intro_msg_update_async(IntroMessageUpdateReqDto introMsgDto)
	{
		var parameters = new { introMsgDto.user_id, introMsgDto.default_intro_message };
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<IntroMessageUpdateReqDto>("usp_business_default_intro_msg_update", parameters, null, null, commandType);
	}
}

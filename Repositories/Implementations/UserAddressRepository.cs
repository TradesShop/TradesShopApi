using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Implementations;

public class UserAddressRepository : IUserAddressRepository
{
	private readonly DapperContext _context;

	public UserAddressRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<Guid> CreateCustomerProfileAsync(RegisterDto reg_dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { reg_dto.user_id, reg_dto.address_line1, reg_dto.address_line2, reg_dto.town, reg_dto.county, reg_dto.postcode, reg_dto.country_id, reg_dto.longitude, reg_dto.latitude };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Guid>("usp_user_customer_profile_create", param, null, null, commandType);
	}

	public async Task<Guid> CreateTradeUserBusinessAsync(RegisterDto reg_dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			reg_dto.user_id, reg_dto.business_name, reg_dto.address_line1, reg_dto.address_line2, reg_dto.town, reg_dto.county, reg_dto.postcode, reg_dto.country_id, reg_dto.longitude, reg_dto.latitude,
			reg_dto.primarytrade, reg_dto.secondarytrade, reg_dto.public_slug
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Guid>("usp_user_trade_business_create", param, null, null, commandType);
	}

	public async Task<IEnumerable<UserAddress>> GetByEntityAsync(Guid entity_id)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new { entity_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<UserAddress>("usp_user_trade_addresses_get_async", param, null, null, commandType);
	}
}

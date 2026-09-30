using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations;

public class BusinessRepository : IBusinessRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public BusinessRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}

	public async Task<UserAddress> BusinessPrimaryAddressForUserId(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<UserAddress>("[dbo].[usp_user_business_primary_addr_get_by_id]", param, null, null, commandType);
	}

	public async Task<IEnumerable<UserAddress>> BusinessAddressesForUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<UserAddress>("usp_user_business_addresses_get_async", param, null, null, commandType);
	}

	public async Task<IEnumerable<UserAddress>> BusinessAdressRemoveAsync(AddressRemoveReq uaModel)
	{
		var parameters = new { uaModel.user_id, uaModel.business_id, uaModel.address_id };
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<UserAddress>("usp_user_business_address_remove", parameters, null, null, commandType);
	}

	public async Task<List<BusinessCategorySkillFlatDto>> GetBusinessCategorySkillsAsync(Guid business_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { business_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return (await conn.QueryAsync<BusinessCategorySkillFlatDto>("usp_user_business_category_skills_get", param, null, null, commandType)).ToList();
	}

	public async Task BusinessSkillsUpdateAsync(BusinessSkillsUpdateDto dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		string skillsCsv = ((dto.skills_ids != null && dto.skills_ids.Any()) ? string.Join(",", dto.skills_ids) : string.Empty);
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@id", dto.id);
		parameters.Add("@user_id", _identity.GetUserId());
		parameters.Add("@business_id", dto.business_id);
		parameters.Add("@category_id", dto.category_id);
		parameters.Add("@skills_ids", skillsCsv);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_user_business_skills_update", parameters, null, null, commandType);
	}

	public async Task<IEnumerable<BusinessCategoryDto>> BusinessCategoryForUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<BusinessCategoryDto>("usp_user_business_category_get_all", param, null, null, commandType);
	}

	public async Task<BusinessProfileDto> BusinessProfileForUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<BusinessProfileDto>("usp_user_business_profile_get_async", param, null, null, commandType);
	}

	public async Task<BusinessProfileDto> BusinessProfileForSlugAsync(string public_slug)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { public_slug };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<BusinessProfileDto>("usp_user_business_profile_get_by_slug", param, null, null, commandType);
	}

	public async Task<BusinessProfileDto> BusinessProfileUpsertAsync(BusinessProfileDto bpDto)
	{
		var parameters = new
		{
			bpDto.id, bpDto.user_id, bpDto.name, bpDto.description, bpDto.active_since, bpDto.website_url, bpDto.business_type_id, bpDto.number_of_employees, bpDto.registration_number, bpDto.service_radius_km,
			bpDto.public_slug
		};
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<BusinessProfileDto>("usp_user_business_profile_upsert", parameters, null, null, commandType);
	}

	public async Task<UserAddress> BusinessAdressUpdateAsync(UserAddress uaModel)
	{
		var parameters = new
		{
			uaModel.user_id, uaModel.business_id, uaModel.address_id, uaModel.address_line1, uaModel.address_line2, uaModel.town, uaModel.county, uaModel.postcode, uaModel.country_code, uaModel.country_id,
			uaModel.latitude, uaModel.longitude, uaModel.address_type_id, uaModel.service_radius_km, uaModel.is_primary
		};
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<UserAddress>("usp_user_trade_address_upsert", parameters, null, null, commandType);
	}

	public async Task<IEnumerable<BusinessWebProfileGetDto>> BusinessWebProfileForUserAsync(Guid business_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { business_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<BusinessWebProfileGetDto>("usp_user_business_web_profile_get", param, null, null, commandType);
	}

	public async Task BusinessWebProfileUpsert(BusinessWebProfileDto bwpDto)
	{
		List<(string, string)> platforms = new List<(string, string)>
		{
			("twitter", bwpDto.twitter_url),
			("facebook", bwpDto.facebook_url)
		};
		foreach (var item in platforms)
		{
			if (!string.IsNullOrWhiteSpace(item.Item2))
			{
				using IDbConnection conn = _context.CreateOpenConnection();
				var param = new
				{
					business_id = bwpDto.business_id,
					platform = item.Item1,
					url = item.Item2
				};
				CommandType? commandType = CommandType.StoredProcedure;
				await conn.ExecuteAsync("usp_user_business_web_profile_upsert", param, null, null, commandType);
			}
		}
	}

	public async Task<int> BusinessMaxCategoriesForUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<int>("usp_user_business_max_categories_get", param, null, null, commandType);
	}

	public async Task<int> BusinessMaxLocationsForUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<int>("usp_user_business_max_locations_get", param, null, null, commandType);
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.PlanPrices;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class PlansRepository : IPlansRepository
{
	private readonly DapperContext _context;

	public PlansRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<PlanPriceByPriceId?> GetPlanPriceByPriceId(Guid plan_price_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { plan_price_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PlanPriceByPriceId>("usp_plan_price_get_by_price_id", param, null, null, commandType);
	}

	public async Task<Plan?> GetPlanByIdAsync(Guid id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Plan>("usp_plans_get_by_id", param, null, null, commandType);
	}

	public async Task<PlanPrice?> GetPlanPriceByIdAsync(Guid id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PlanPrice>("usp_plan_prices_get_by_plan", param, null, null, commandType);
	}

	public async Task<IEnumerable<Plan>> GetAllPlansAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<Plan>("usp_plans_get_all", null, null, null, commandType);
	}

	public async Task<IEnumerable<PlanPrice>> GetPlanPricesAsync(Guid planId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			plan_id = planId
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PlanPrice>("usp_plan_prices_get_by_plan", param, null, null, commandType);
	}

	public async Task<List<Plan>> GetActivePlansAsync(string plan_type, Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id, plan_type };
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await conn.QueryMultipleAsync("[dbo].[usp_plans_list_active_for_user]", param, null, null, commandType);
		List<Plan> plans = (await multi.ReadAsync<Plan>()).ToList();
		List<PlanPrice> prices = (await multi.ReadAsync<PlanPrice>()).ToList();
		foreach (Plan plan in plans)
		{
			PlanPrice activePrice = prices.Where((PlanPrice p) => p.plan_id == plan.id && p.is_active).FirstOrDefault();
			plan.active_price = activePrice;
		}
		return plans;
	}

	public async Task<IEnumerable<PlansMeta>> GetPlansListAsync(string? searchname, string? type)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { searchname, type };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PlansMeta>("[dbo].[usp_plans_list]", param, null, null, commandType);
	}

	public async Task<IEnumerable<PlansMeta>> PlansExistsAsync(string? name, Guid? exclude_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			name = name,
			exclude_plan_id = exclude_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PlansMeta>("[dbo].[usp_plan_name_exists]", param, null, null, commandType);
	}

	public async Task<PlansMeta> PlansUpsertAsync(PlansMeta anyplan)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			anyplan.id, anyplan.code, anyplan.name, anyplan.trade_categories, anyplan.is_active, anyplan.sort_order, anyplan.is_highlighted, anyplan.highlight_label, anyplan.is_vatable, anyplan.type,
			anyplan.expiry_months
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PlansMeta>("[dbo].[usp_plan_upsert]", param, null, null, commandType);
	}

	public async Task<IEnumerable<PlanPricesMeta>> GetPlanPricesByPlanIdAsync(Guid plan_id, bool active_only = false)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { plan_id, active_only };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PlanPricesMeta>("[dbo].[usp_plan_prices_get_by_plan_id]", param, null, null, commandType);
	}

	public async Task<PlanPricesMeta> PlanPricesUpsertAsync(PlanPricesMeta anyprice)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			anyprice.id, anyprice.plan_id, anyprice.price, anyprice.currency, anyprice.billing_interval, anyprice.stripe_price_id, anyprice.stripe_product_id, anyprice.credits_per_period, anyprice.extra_categories, anyprice.extra_locations,
			anyprice.boost_score, anyprice.is_recurring, anyprice.is_active, anyprice.valid_from, anyprice.valid_to, anyprice.description
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PlanPricesMeta>("[dbo].[usp_plan_price_upsert]", param, null, null, commandType);
	}
}

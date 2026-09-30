using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.BundleCredit;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class BundlesCreditRepository : IBundlesCreditRepository
{
	private readonly DapperContext _context;

	public BundlesCreditRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<CreditBundles>> GetAllBundlesAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await conn.QueryMultipleAsync("usp_credit_bundles_get_active_full", null, null, null, commandType);
		List<CreditBundles> credit_bundles = (await multi.ReadAsync<CreditBundles>()).ToList();
		List<BundlePrices> bundle_prices = (await multi.ReadAsync<BundlePrices>()).ToList();
		foreach (CreditBundles credit_b in credit_bundles)
		{
			BundlePrices activePrice = bundle_prices.Where((BundlePrices bp) => bp.bundle_id == credit_b.id && bp.is_active).FirstOrDefault();
			credit_b.active_price = activePrice;
		}
		return credit_bundles;
	}

	public async Task<CreditBundles?> GetByIdAsync(Guid bundle_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = bundle_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<CreditBundles>("usp_credit_bundles_get_by_id", param, null, null, commandType);
	}

	public async Task CreateAsync(CreditBundles model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.name, model.expiry_months, model.is_active, model.created_at };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_credit_bundles_create", param, null, null, commandType);
	}

	public async Task UpdateAsync(CreditBundles model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.name, model.expiry_months, model.is_active };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_credit_bundles_update", param, null, null, commandType);
	}

	public async Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		return await conn.QueryAsync<CreditBundles>("\r\n        SELECT *\r\n        FROM credit_bundles\r\n        WHERE is_active = 1\r\n        ORDER BY created_at DESC;\r\n    ");
	}
}

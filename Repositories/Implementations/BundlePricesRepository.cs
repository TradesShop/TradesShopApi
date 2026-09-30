using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models.BundleCredit;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class BundlePricesRepository : IBundlePricesRepository
{
	private readonly DapperContext _context;

	public BundlePricesRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<BundlePrices> GetPricesByIdAsync(Guid bundle_price_id)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new
		{
			id = bundle_price_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<BundlePrices>("usp_bundle_prices_get_by_id", param, null, null, commandType);
	}

	public async Task CreateAsync(BundlePrices model)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new { model.id, model.bundle_id, model.price, model.currency, model.stripe_price_id, model.is_active, model.is_vatable, model.created_at };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_bundle_prices_create", param, null, null, commandType);
	}

	public async Task UpdateAsync(BundlePrices model)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new { model.id, model.price, model.currency, model.stripe_price_id, model.is_active, model.is_vatable };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_bundle_prices_update", param, null, null, commandType);
	}
}

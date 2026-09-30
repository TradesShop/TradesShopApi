using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Dashboard;
using TradePlatform.Api.DTOs.MasterData;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class MasterDataRepository : IMasterDataRepository
{
	private readonly DapperContext _context;

	public MasterDataRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<MasterDataStatuses>> MasterDataStatusesAsync(string entity_type, bool? is_active)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { entity_type, is_active };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<MasterDataStatuses>("usp_status_master_get", param, null, null, commandType);
	}

	public async Task<TraderDashboardStatsDto> GetTraderDashboardStatsAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return (await conn.QueryFirstOrDefaultAsync<TraderDashboardStatsDto>("dbo.usp_trader_dashboard_stats_get", param, null, null, commandType)) ?? new TraderDashboardStatsDto();
	}
}

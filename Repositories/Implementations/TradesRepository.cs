using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class TradesRepository : ITradesRepository
{
	private readonly DapperContext _context;

	public TradesRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<Trades>> GetTradesAsync(int? id)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<Trades>("usp_Trades_Get", param, null, null, commandType);
	}
}

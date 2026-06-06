using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class TradesRepository : ITradesRepository
    {
      
            private readonly DapperContext _context;

            public TradesRepository(DapperContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Trades>> GetTradesAsync(int? id)
            {
                using var conn = _context.CreateConnection();

                return await conn.QueryAsync<Trades>(
                    "usp_Trades_Get",
                    new { id = id },
                    commandType: CommandType.StoredProcedure
                );
            }

    }
}

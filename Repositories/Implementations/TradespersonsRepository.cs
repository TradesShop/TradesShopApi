using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class TradespersonsRepository : ITradespersonsRepository
    {
        private readonly DapperContext _context;

        public TradespersonsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Tradesperson tradesperson)
        {
            tradesperson.Id = Guid.NewGuid();
            tradesperson.Created_At = DateTime.UtcNow;
            tradesperson.Updated_At = DateTime.UtcNow;

            using var conn = _context.CreateConnection();

            await conn.ExecuteAsync(
                "spTradespersons_Create",
                new
                {
                    tradesperson.Id,
                    tradesperson.User_Id,
                    tradesperson.Company_Name,
                    tradesperson.Bio,
                    tradesperson.Years_Experience,
                    tradesperson.Postcode,
                    tradesperson.Address,
                    tradesperson.Country_Id,
                    tradesperson.GLng,
                    tradesperson.GLat,
                    tradesperson.Public_Liability_Insurance,
                    tradesperson.Verified,
                    tradesperson.Created_At,
                    tradesperson.Updated_At
                },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}

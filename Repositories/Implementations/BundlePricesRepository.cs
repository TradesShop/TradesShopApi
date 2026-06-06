using System.Data;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class BundlePricesRepository : IBundlePricesRepository
    {
        private readonly DapperContext _context;

        public BundlePricesRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<BundlePrices> GetPricesByIdAsync(Guid bundle_price_id)
        {
            using var conn = _context.CreateConnection();
            var anyprice= await conn.QueryFirstOrDefaultAsync<BundlePrices>(
                "usp_bundle_prices_get_by_id",
                new { id=bundle_price_id },
                commandType: CommandType.StoredProcedure
            );
            return anyprice;
        }


        public async Task CreateAsync(BundlePrices model)
        {
            using var conn = _context.CreateConnection();

            await conn.ExecuteAsync(
                "usp_bundle_prices_create",
                new
                {
                    model.id,
                    model.bundle_id,
                    model.price,
                    model.currency,
                    model.stripe_price_id,
                    model.is_active,
                    model.is_vatable,
                    model.created_at
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateAsync(BundlePrices model)
        {
            using var conn = _context.CreateConnection();

            await conn.ExecuteAsync(
                "usp_bundle_prices_update",
                new
                {
                    model.id,
                    model.price,
                    model.currency,
                    model.stripe_price_id,
                    model.is_active,
                    model.is_vatable
                },
                commandType: CommandType.StoredProcedure
            );
        }
       

    }
}

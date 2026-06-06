using Dapper;
using System.Data;
using System.Reflection.Metadata;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class BundlesCreditRepository : IBundlesCreditRepository
    {
        private readonly DapperContext _context;

        public BundlesCreditRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CreditBundles>> GetAllBundlesAsync()
        {
            using var conn = _context.CreateOpenConnection();
           
            using var multi = await conn.QueryMultipleAsync(
                "usp_credit_bundles_get_active_full",
                commandType: CommandType.StoredProcedure
            );

            // Result set 1 → Plans
            var credit_bundles = (await multi.ReadAsync<CreditBundles>()).ToList();

            // Result set 2 → Prices
            var bundle_prices = (await multi.ReadAsync<BundlePrices>()).ToList();

            // Attach prices to each plan
            foreach (var credit_b in credit_bundles)
            {
                // Find the active price for this plan
                var activePrice = bundle_prices
                    .Where(bp => bp.bundle_id == credit_b.id && bp.is_active)
                    //.OrderByDescending(p => p.created_at) // optional
                    .FirstOrDefault();

                credit_b.active_price = activePrice;

                // Remove the old list if you don't want it
                //plan.prices = null;
            }

            return credit_bundles;
        }

        public async Task<CreditBundles?> GetByIdAsync(Guid bundle_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<CreditBundles>(
                "usp_credit_bundles_get_by_id",
                new { id = bundle_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task CreateAsync(CreditBundles model)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_credit_bundles_create",
                new
                {
                    model.id,
                    model.name,
                   // model.credits,
                    model.expiry_months,
                    model.is_active,
                    model.created_at
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateAsync(CreditBundles model)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_credit_bundles_update",
                new
                {
                    model.id,
                    model.name,
                    //model.credits,
                    model.expiry_months,
                    model.is_active
                },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync()
        {
            const string sql = @"
        SELECT *
        FROM credit_bundles
        WHERE is_active = 1
        ORDER BY created_at DESC;
    ";

            using var conn = _context.CreateOpenConnection();
            return await conn.QueryAsync<CreditBundles>(sql);
        }
    }
}

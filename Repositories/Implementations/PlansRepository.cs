using Dapper;
using System.Data;
using System.Text.Json;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class PlansRepository: IPlansRepository
    {
        private readonly DapperContext _context;

        public PlansRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<PlanPriceByPriceId?> GetPlanPriceByPriceId(Guid plan_price_id)
        {
            using var conn = _context.CreateOpenConnection();
            return await conn.QueryFirstOrDefaultAsync<PlanPriceByPriceId>(
                "usp_plan_price_get_by_price_id",
                new { plan_price_id= plan_price_id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<Plan?> GetPlanByIdAsync(Guid id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<Plan>(
                "usp_plans_get_by_id",
                new { id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<PlanPrice?> GetPlanPriceByIdAsync(Guid id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<PlanPrice>(
                "usp_plan_prices_get_by_plan",
                new { id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Plan>> GetAllPlansAsync()
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<Plan>(
                "usp_plans_get_all",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<PlanPrice>> GetPlanPricesAsync(Guid planId)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<PlanPrice>(
                "usp_plan_prices_get_by_plan",
                new { plan_id = planId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<List<Plan>> GetActivePlansAsync()
        {
            using var conn = _context.CreateOpenConnection();

            using var multi = await conn.QueryMultipleAsync(
                "usp_plans_get_active_full",
                commandType: CommandType.StoredProcedure
            );

            // Result set 1 → Plans
            var plans = (await multi.ReadAsync<Plan>()).ToList();

            // Result set 2 → Prices
            var prices = (await multi.ReadAsync<PlanPrice>()).ToList();

            // Attach prices to each plan
            foreach (var plan in plans)
            {
                // Find the active price for this plan
                var activePrice = prices
                    .Where(p => p.plan_id == plan.id && p.is_active)
                    //.OrderByDescending(p => p.created_at) // optional
                    .FirstOrDefault();

                plan.active_price = activePrice;

                // Remove the old list if you don't want it
                //plan.prices = null;
            }

            return plans;
        }
    }
}

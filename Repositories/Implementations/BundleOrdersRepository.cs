using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class BundleOrdersRepository : IBundleOrdersRepository
    {
        private readonly DapperContext _context;

        public BundleOrdersRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<BundleOrders> CreateAsync(BundleOrders order)
        {
            using var conn = _context.CreateOpenConnection();

            var anyorder = await conn.QueryFirstOrDefaultAsync<BundleOrders>(
                "usp_bundle_order_create",
                new
                {
                    order.user_id,
                    order.bundle_price_id,
                    order.stripe_session_id,
                    order.stripe_price_id,
                    order.amount,
                    order.currency
                },
                commandType: CommandType.StoredProcedure
            );
            return anyorder;
        }
        public async Task BundleCheckoutCompletedAsync(BundleCheckoutCompletedDto dto)
        {
            using var conn = _context.CreateOpenConnection();
            var p = new DynamicParameters();

            p.Add("@bundle_order_id", dto.bundle_order_id);
            p.Add("@bundle_price_id", dto.bundle_price_id);
            p.Add("@user_id", dto.user_id);

            p.Add("@stripe_payment_intent_id", dto.stripe_payment_intent_id);
            p.Add("@stripe_customer_id", dto.stripe_customer_id);
            p.Add("@customer_email", dto.customer_email);

            p.Add("@amount_total", dto.amount_total);
            p.Add("@amount_subtotal", dto.amount_subtotal);
            p.Add("@currency", dto.currency);
            p.Add("@metadata_json", dto.metadataJson);

            await conn.ExecuteAsync(
                "usp_bundle_checkout_completed",
                p,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task BundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto)
        {
            using var conn = _context.CreateOpenConnection();
            var p = new DynamicParameters();

            p.Add("@bundle_order_id", dto.bundle_order_id);            

            await conn.ExecuteAsync(
                "usp_bundle_order_mark_failed",
                p,
                commandType: CommandType.StoredProcedure
            );
        }        
        

        public async Task MarkPaidAsync(string stripe_session_id, string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_bundle_order_mark_paid",
                new
                {
                    stripe_session_id,
                    stripe_payment_intent_id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkRefundedAsync(string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_bundle_order_mark_refunded",
                new
                {
                    stripe_payment_intent_id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<BundleOrders?> GetByStripeSessionIdAsync(string stripe_session_id)
        {
            using var conn = _context.CreateOpenConnection();

            var sql = "SELECT * FROM bundle_orders WHERE stripe_session_id = @stripe_session_id";
            return await conn.QueryFirstOrDefaultAsync<BundleOrders>(sql, new { stripe_session_id });
        }

        public async Task<BundleOrders?> GetByPaymentIntentIdAsync(string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            var sql = "SELECT * FROM bundle_orders WHERE stripe_payment_intent_id = @stripe_payment_intent_id";
            return await conn.QueryFirstOrDefaultAsync<BundleOrders>(sql, new { stripe_payment_intent_id });
        }
    }
}

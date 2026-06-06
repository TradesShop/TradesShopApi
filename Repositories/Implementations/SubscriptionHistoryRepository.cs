using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class SubscriptionHistoryRepository: ISubscriptionHistoryRepository
    {
        private readonly DapperContext _context;

        public SubscriptionHistoryRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<long> SubscriptionHistoryInsertAsync(SubscriptionHistory h)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                h.subscription_id,
                h.from_plan_price_id,
                h.to_plan_price_id,
                h.action,
                h.reason,
                h.effective_date,
                h.source_system,
                h.source_id,
                h.stripe_invoice_id,
                h.stripe_event_id,
                h.metadata
            };

            return await connection.ExecuteScalarAsync<long>(
                "usp_subscription_history_insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
//status will be created,plan_changed,renewed,canceled,period_updated,trial_started,trial_ended,status_changed
//reason wiil be “User upgraded”,“Stripe webhook”,“Payment failed”,“Admin action”,“User canceled”,“Trial ended”
//source_system will be api,stripe,admin,system
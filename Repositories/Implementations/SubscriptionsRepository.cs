using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Data;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class SubscriptionsRepository : ISubscriptionsRepository
    {
        private readonly DapperContext _context;

        public SubscriptionsRepository(DapperContext context)
        {
            _context = context;
        }
        
        public async Task SubscriptionEventProcessUpdateAsync(SubscriptionEventProcessDto model)
        {
            using var conn = _context.CreateOpenConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);
            parameters.Add("@user_id", model.user_id);
            parameters.Add("@plan_price_id", model.plan_price_id);
           
            parameters.Add("@current_period_start", model.current_period_start);
            parameters.Add("@current_period_end", model.current_period_end);
            parameters.Add("@status", model.status);
            parameters.Add("@cancel_at_period_end", model.cancel_at_period_end);
            parameters.Add("@trial_end", model.trial_end);
            parameters.Add("@metadata_json", model.metadata_json);
            parameters.Add("@stripe_event_id", model.stripe_event_id);
            parameters.Add("@event_type", model.event_type);
            parameters.Add("@actor", model.actor);
            parameters.Add("@source", model.source);
            await conn.ExecuteAsync(
               "dbo.usp_subscription_event_process_update",
               parameters,
               commandType: CommandType.StoredProcedure
           );
        }
        public async Task<SubscriptionViewDto?> GetActiveSubscriptionForUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<SubscriptionViewDto>(
                "usp_subscriptions_get_by_user",
                new { user_id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<Subscriptions?> GetByStripeIdAsync(string stripe_subscriptionid)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<Subscriptions>(
                "usp_subscriptions_get_by_stripe_id",
                new { stripe_subscription_id=stripe_subscriptionid },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<Subscriptions> InsertSubscriptionAsync(Subscriptions model)
        {
            using var conn = _context.CreateOpenConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@user_id", model.user_id);
            parameters.Add("@plan_price_id", model.plan_price_id);
            parameters.Add("@status", model.status);
            parameters.Add("@current_period_start", model.current_period_start);
            parameters.Add("@current_period_end", model.current_period_end);
            parameters.Add("@auto_renew", model.auto_renew);
            parameters.Add("@stripe_customer_id", model.stripe_customer_id);
            parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);

            var subscription = await conn.QueryFirstAsync<Subscriptions>(
                "usp_subscriptions_insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return subscription;
        }

     

        public async Task<Subscriptions> SubscriptionUpdatePriceAsync(
            string stripe_subscription_id,
            string stripe_price_id,
            DateTime current_period_start,
            DateTime current_period_end)
        {
            using var conn = _context.CreateOpenConnection();

            var anysub=await conn.QueryFirstOrDefaultAsync<Subscriptions>(
                "usp_subscriptions_update_price",
                new
                {
                    stripe_subscription_id = stripe_subscription_id,
                    stripe_price_id= stripe_price_id,
                    current_period_start= current_period_start,
                    current_period_end= current_period_end
                },
                commandType: CommandType.StoredProcedure
            );
            return anysub;
        }

        // existing methods omitted for brevity...

        public async Task MarkActiveAsync(string stripe_subscription_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_subscriptions_mark_active",
                new { stripe_subscription_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkPastDueAsync(string stripe_subscription_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_subscriptions_mark_past_due",
                new { stripe_subscription_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkCanceledAsync(string stripe_subscription_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_subscriptions_mark_canceled",
                new { stripe_subscription_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdatePeriodAsync(
            string stripe_subscription_id,
            DateTime current_period_start,
            DateTime current_period_end,
            string status,
            bool cancel_at_period_end)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_subscriptions_update_period",
                new
                {
                    stripe_subscription_id,
                    current_period_start,
                    current_period_end,
                    status,
                    cancel_at_period_end
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}

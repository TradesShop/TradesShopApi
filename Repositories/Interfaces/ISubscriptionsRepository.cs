using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ISubscriptionsRepository
    {
        Task<SubscriptionViewDto?> GetActiveSubscriptionForUserAsync(Guid user_id);
        Task SubscriptionEventProcessUpdateAsync(SubscriptionEventProcessDto model);
        
        Task<Subscriptions?> GetByStripeIdAsync(string stripe_subscriptionid);
        
        //Task UpdateAsync(Subscriptions subscription);
        Task<Subscriptions> InsertSubscriptionAsync(Subscriptions model);
        Task<Subscriptions> SubscriptionUpdatePriceAsync(
            string stripe_subscription_id,
            string stripe_price_id,
            DateTime current_period_start,
            DateTime current_period_end
        );
        // Webhook-related
        Task MarkActiveAsync(string stripe_subscription_id);
        Task MarkPastDueAsync(string stripe_subscription_id);
        Task MarkCanceledAsync(string stripe_subscription_id);
        Task UpdatePeriodAsync(string stripe_subscription_id, DateTime start, DateTime end, string status, bool cancelAtPeriodEnd);

    }
}

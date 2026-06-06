using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.DTOs.subscription;

namespace TradePlatform.Api.Services.Subscriptions
{
    public interface IUserSubscriptionService
    {
        Task<SubscriptionViewDto> GetActiveSubscriptionForUserAsync(Guid user_id);
        //Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid user_id);

        //Task<SubscriptionDto> CreateOrUpdateSubscriptionAsync(
        //    Guid user_id,
        //    string price_id,
        //    Guid? updated_by
        //);

        //Task CancelSubscriptionAsync(
        //    Guid user_id,
        //    Guid? updated_by
        //);
    }
}

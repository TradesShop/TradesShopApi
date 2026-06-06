using TradePlatform.Api.DTOs.subscription;

namespace TradePlatform.Api.Services
{
    public interface IBillingServices
    {
        Task<SubscriptionSelectResponse> SelectSubscriptionAsync(
        Guid user_id,
        Guid plan_id,
        Guid plan_price_id
    );

        Task<SubscriptionSelectResponse> CreateSubscriptionAsync(
            Guid user_id,
            Guid plan_id,
            Guid plan_price_id
        );
    }
}

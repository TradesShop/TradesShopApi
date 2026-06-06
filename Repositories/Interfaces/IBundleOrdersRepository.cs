using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IBundleOrdersRepository
    {
        Task<BundleOrders> CreateAsync(BundleOrders order);
        Task BundleCheckoutCompletedAsync(BundleCheckoutCompletedDto dto);
        Task BundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto);        
        Task MarkPaidAsync(string stripe_session_id, string stripe_payment_intent_id);
        Task MarkRefundedAsync(string stripe_payment_intent_id);
        Task<BundleOrders?> GetByStripeSessionIdAsync(string stripe_session_id);
        Task<BundleOrders?> GetByPaymentIntentIdAsync(string stripe_payment_intent_id);
       
    }
}

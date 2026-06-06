using Stripe;
using System;
using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services
{
    public interface IStripeService
    {
        Event VerifyWebhook(string json, string? signature);
        Task<string> ResolveStripeCustomerIdAsync(Guid user_id);
        Task<PaymentMethod_db> AttachPaymentMethodToCustomerAsync(
            Guid user_id,
            string paymentmethod_id
        );

        Task SetDefaultPaymentMethodAsync(
            Guid user_id,
            string stripe_payment_method_id
        );

        Task DetachPaymentMethodAsync(
            Guid user_id,
            string paymentmethod_id
        );

        Task<Subscription> CreateOrUpdateSubscriptionAsync(
            Guid user_id,
            string price_id,
            string paymentmethod_id
        );

        Task CancelSubscriptionAsync(
            string stripe_subscriptionid
        );

        Task<string> CreateSetupIntentAsync(Guid user_id);

        Task UpdatePaymentMethodAsync(
            string payment_method_id,
            string? name_on_card,
            int expMonth,
            int expYear);
        
    }
}

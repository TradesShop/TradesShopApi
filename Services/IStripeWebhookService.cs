using System.Threading.Tasks;
using Stripe;

namespace TradePlatform.Api.Services
{
    public interface IStripeWebhookService
    {
        // Main Stripe webhook entrypoint
        Task HandleEventAsync(Event stripeEvent, string rawJson, string? signature);

        // Manual / API‑initiated refund (e.g. admin panel, support tool)
        Task HandleRefundAsync(
            string stripe_payment_intent_id,
            decimal amount,
            string reason,
            string reference_type,
            string reference_id,
            string user_id
        );
    }
}

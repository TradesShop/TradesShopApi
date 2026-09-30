using System.Threading.Tasks;
using Stripe;

namespace TradePlatform.Api.Services;

public interface IStripeWebhookService
{
	Task HandleEventAsync(Event stripeEvent, string rawJson, string? signature);

	Task HandleRefundAsync(string stripe_payment_intent_id, decimal amount, string reason, string reference_type, string reference_id, string user_id);
}

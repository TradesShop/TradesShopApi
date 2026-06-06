namespace TradePlatform.Api.DTOs.subscription
{
    public class SubscriptionSelectResponse
    {
        public bool requires_payment_method { get; set; }
        public string? client_secret { get; set; }
        public bool ready_for_subscription { get; set; }
        public Guid? subscription_id { get; set; }
        public string stripe_subscription_id { get; set; }
        public string status { get; set; }
    }
}

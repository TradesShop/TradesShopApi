namespace TradePlatform.Api.DTOs.subscription
{
    public class SubscriptionCreateResponse
    {
        public string status { get; set; }
        public string? client_secret { get; set; }
        public Guid subscription_id { get; set; }
    }
}

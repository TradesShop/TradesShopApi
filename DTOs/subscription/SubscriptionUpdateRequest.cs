namespace TradePlatform.Api.DTOs.subscription
{
    public class SubscriptionUpdateRequest
    {
        public Guid subscription_id { get; set; }        
        public Guid plan_price_id { get; set; }
    }
}

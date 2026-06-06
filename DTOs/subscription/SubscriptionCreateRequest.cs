namespace TradePlatform.Api.DTOs.subscription
{
    public class SubscriptionCreateRequest
    {
        public Guid? target_user_id { get; set; }
        public Guid plan_id { get; set; }
        public Guid plan_price_id { get; set; }
    }
    
    

}

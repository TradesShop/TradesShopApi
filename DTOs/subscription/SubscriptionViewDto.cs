namespace TradePlatform.Api.DTOs.subscription
{
    public class SubscriptionViewDto
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid plan_price_id { get; set; }
        public string plan_name { get; set; }
        public Guid plan_id { get; set; }
        public int credits_per_period { get; set; }
        public string status { get; set; } = null!;
        public DateTime? current_period_start { get; set; }
        public DateTime? current_period_end { get; set; }
        public bool auto_renew { get; set; }
        public string stripe_customer_id { get; set; }
        public string stripe_subscription_id { get; set; } = null!;
        public string stripe_priceid { get; set; } = null!;
        public bool cancel_at_period_end { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

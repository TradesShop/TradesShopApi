namespace TradePlatform.Api.Models
{
    public class PlanPrice
    {
        public Guid id { get; set; }
        public Guid plan_id { get; set; }
        public decimal price { get; set; }
        public string currency { get; set; }
        public string billing_interval { get; set; }
        public string stripe_price_id { get; set; }
        public bool is_active { get; set; }
        public int  credits_per_period { get; set; }
    }
}

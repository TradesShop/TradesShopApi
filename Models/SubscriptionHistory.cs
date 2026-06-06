namespace TradePlatform.Api.Models
{
    public class SubscriptionHistory
    {
        public long id { get; set; }
        public Guid subscription_id { get; set; }

        public Guid? from_plan_price_id { get; set; }
        public Guid? to_plan_price_id { get; set; }

        public string action { get; set; } = string.Empty;
        public string? reason { get; set; }

        public DateTime effective_date { get; set; }

        public string? source_system { get; set; }
        public Guid? source_id { get; set; }

        public string? stripe_invoice_id { get; set; }
        public string? stripe_event_id { get; set; }

        public string? metadata { get; set; }

        public DateTime created_at { get; set; }
    }
}

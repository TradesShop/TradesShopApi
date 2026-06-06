namespace TradePlatform.Api.Models
{
    public class BundleOrders
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid bundle_price_id { get; set; }

        public string? stripe_session_id { get; set; }
        public string? stripe_payment_intent_id { get; set; }
        public string? stripe_price_id { get; set; }

        public decimal? amount { get; set; }
        public string? currency { get; set; }

        public string status { get; set; } = string.Empty; // pending / paid / refunded

        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}

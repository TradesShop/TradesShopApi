namespace TradePlatform.Api.Models
{
    public class BundlePrices
    {
        public Guid id { get; set; }
        public Guid bundle_id { get; set; }
        public decimal price { get; set; }
        public int credits { get; set; }
        public string currency { get; set; } = string.Empty;
        public string stripe_price_id { get; set; } = string.Empty;
        public bool is_active { get; set; }
        public bool is_vatable { get; set; }
        public DateTime created_at { get; set; }
    }
}

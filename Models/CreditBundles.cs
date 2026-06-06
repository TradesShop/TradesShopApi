namespace TradePlatform.Api.Models
{
    public class CreditBundles
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; }
        public int sort_order { get; set; }        
        public int expiry_months { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public BundlePrices active_price { get; set; }
}
}

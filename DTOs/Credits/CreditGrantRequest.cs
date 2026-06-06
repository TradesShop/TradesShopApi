namespace TradePlatform.Api.DTOs.Credits
{
    public class CreditGrantRequest
    {
        public Guid user_id { get; set; }
        public string source { get; set; } = string.Empty;          // subscription / bundle / admin / promotion / refund
        public Guid? reference_id { get; set; }                     // subscription_id / bundle_id / promo_id / dispute_id
        public int total_credits { get; set; }
        public DateTime expires_at { get; set; }
        public string? reference_type { get; set; }                 // subscription / bundle / promotion / dispute
        public string? metadata { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.Credits
{
    public class CreditRefundRequest
    {
        public Guid user_id { get; set; }
        public int credits_to_refund { get; set; }
        public string reference_type { get; set; } = string.Empty;  // dispute / job
        public Guid reference_id { get; set; }                      // dispute_id or job_id
        public DateTime expires_at { get; set; }
        public string? metadata { get; set; }
    }
}

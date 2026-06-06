namespace TradePlatform.Api.DTOs.Credits
{
    public class CreditConsumeRequest
    {
        public Guid user_id { get; set; }
        public int credits_to_use { get; set; }
        public string reference_type { get; set; } = string.Empty;  // job
        public Guid reference_id { get; set; }                      // job_id
        public string? metadata { get; set; }
    }
}

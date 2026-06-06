namespace TradePlatform.Api.Models
{
    public class UserCredits
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid subscription_id { get; set; }
        public int credits_allocated { get; set; }
        public int credits_used { get; set; }
        public DateTime period_start { get; set; }
        public DateTime period_end { get; set; }
        public DateTime created_at { get; set; }
    }
}

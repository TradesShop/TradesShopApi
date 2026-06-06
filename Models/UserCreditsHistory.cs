namespace TradePlatform.Api.Models
{
    public class UserCreditsHistory
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string? source_system { get; set; }
        public Guid? source_id { get; set; }
        public int entity_type { get; set; }
        public Guid? entity_id { get; set; }
        public int credits_change { get; set; }
        public int balance_before { get; set; }
        public int balance_after { get; set; }
        public string? reason_code { get; set; }
        public string? reason { get; set; }
        public DateTime created_at { get; set; }
    }
}

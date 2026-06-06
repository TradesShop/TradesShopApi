namespace TradePlatform.Api.Models
{
    public class StripeEvents
    {
        public int id { get; set; }

        public string event_id { get; set; } = null!;
        public string event_type { get; set; } = null!;
        public string? api_version { get; set; }
        public bool livemode { get; set; }

        public string payload { get; set; } = null!;
        public string? signature { get; set; }

        public bool processed { get; set; }
        public DateTime? processed_at { get; set; }

        public DateTime received_at { get; set; }
    }
}

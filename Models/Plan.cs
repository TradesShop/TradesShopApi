namespace TradePlatform.Api.Models
{
    public class Plan
    {
        public Guid id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        // public int credits_per_month { get; set; }
        public bool is_vatable { get; set; }
        public bool is_highlighted { get; set; }
        public string highlight_label { get; set; }
        // ⭐ REQUIRED for repository mapping
        public PlanPrice active_price { get; set; }
    }

}


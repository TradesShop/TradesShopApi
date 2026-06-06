namespace TradePlatform.Api.Models
{
    public class plan_feature
    {
        public int? id { get; set; }
        public Guid? plan_id { get; set; }
        public string feature_name { get; set; }
        public int sort_order { get; set; }
       // public DateTime created_at { get; set; }
    }
}

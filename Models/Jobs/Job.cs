namespace TradePlatform.Api.Models.Jobs
{
    public class Job
    {
        public Guid id { get; set; }
        public long uid { get; set; }
        public Guid? user_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int credit_cost { get; set; }
        public string tuser_postcode { get; set; }
        public int status_id { get; set; }
        public int distance_km { get; set; }
        public DateTime created_at { get; set; }
    }
}

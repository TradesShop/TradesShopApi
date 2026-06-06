namespace TradePlatform.Api.DTOs.Jobs
{
    public class MyJobsRequestDto
    {
        public Guid? user_id { get; set; }
        public int? status_id { get; set; }      
        public DateTime? last_created_at { get; set; }
        public int? last_id { get; set; }
        public int limit { get; set; }
    }
}

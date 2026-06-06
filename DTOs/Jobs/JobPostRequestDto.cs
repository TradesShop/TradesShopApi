namespace TradePlatform.Api.DTOs.Jobs
{
    public class JobPostRequestDto
    {
        public Guid user_id { get; set; }
        public int category_id { get; set; }
        public int? sub_category_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int? timeline_id { get; set; }
        public int visibility_id { get; set; }
        public int budget_range_id { get; set; }      
        public string postcode { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public Guid created_by { get; set; }
        public string? ip_address { get; set; }
        public string? user_agent { get; set; }
        public WorkplaceDto workplace { get; set; }
        public List<JobPostAnswerDto> answers { get; set; }
    }
}

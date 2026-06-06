namespace TradePlatform.Api.DTOs.Jobs
{
    public class tUserJobsListRequestDto
    {
        public Guid? target_user_id { get; set; }
        public Guid? user_id { get; set; }
        public int status_id { get; set; }
        public DateTime? last_created_at { get; set; }
        public int? last_id { get; set; }
        public int? limit { get; set; }
        public string sort_key { get; set; }
        public string sort_dir { get; set; }
    }
}

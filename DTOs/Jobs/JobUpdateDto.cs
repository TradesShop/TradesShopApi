namespace TradePlatform.Api.DTOs.Jobs
{
    public class JobUpdateBase
    {
        public Guid job_id { get; set; }
        public Guid? user_id { get; set; }
    }
    public class JobUpdateEntityDto: JobUpdateBase
    {
        public string? action { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public int? budget_range_id { get; set; }
    }

    public class JobUpdateStatus: JobUpdateBase
    {
        public string statuscode { get; set; }
        public Guid? completed_by { get; set; }
        public int? closure_reason_id { get; set; }
        public string? note { get; set; }
    }
}

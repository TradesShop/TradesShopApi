namespace TradePlatform.Api.DTOs.Reviews
{
    public class ReviewSubmitDto
    {
            public Guid? request_id { get; set; }
            public Guid? job_purchase_id { get; set; }
            public Guid job_id { get; set; }
            public Guid? homeowner_id { get; set; }

            public byte overall_rating { get; set; }
            public string? title { get; set; }
            public string? comment { get; set; }
            public bool would_recommend { get; set; }
            

    }
}

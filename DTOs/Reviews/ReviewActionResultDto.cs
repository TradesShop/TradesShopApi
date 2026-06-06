namespace TradePlatform.Api.DTOs.Reviews
{
    public class ReviewActionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Guid? request_id { get; set; }
        public int? review_id { get; set; }
    }
}

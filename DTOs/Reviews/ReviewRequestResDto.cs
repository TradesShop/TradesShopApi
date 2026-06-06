namespace TradePlatform.Api.DTOs.Reviews
{
    public class ReviewRequestResDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Guid? review_request_id { get; set; }
    }
}

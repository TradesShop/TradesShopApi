using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.DTOs.Reviews
{
    public class ReviewReplySubmit
    {
        public Guid user_id { get; set; }
        public Guid review_id { get; set; }
        public string reply_text { get; set; }
    }
    public class ReviewReplyResponse: review_reply_meta
    {
        public Guid user_id { get; set; }
        public Guid review_id { get; set; }
        public string reply_text { get; set; }
        public bool success { get; set; }
        public string message { get; set; }

    }
    
}

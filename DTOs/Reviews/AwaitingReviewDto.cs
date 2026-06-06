using System;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.DTOs.Reviews
{
    public class AwaitingReviewDto
    {

        public Guid id { get; set; }
        public Guid job_purchase_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime created_at { get; set; }
        public string homeowner { get; set; }
        public string postcode { get; set; }
        public DateTime purchased_at { get; set; }
        public WorkplaceDto workplace { get; set; }
        public string workplacejson { get; set; }
        public Guid? review_request_id { get; set; }



    }
}

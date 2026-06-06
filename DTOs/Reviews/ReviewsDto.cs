using System;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.DTOs.Reviews
{
    public class ReviewsDto: review_request_meta
    {

        public Guid job_purchase_id { get; set; }
        public Guid job_id { get; set; }
        public string job_title { get; set; }
        public Guid trade_user_id { get; set; }
        public string trader_first { get; set; }
        public string trader_last { get; set; }
        public string trader_phone { get; set; }
        public string business_name { get; set; }
        public string slug { get; set; }
        public string logoUrl { get; set; }
        public bool would_recommend { get; set; }


        public Guid? msg_id { get; set; }

        public string? msg_text { get; set; }

        public DateTime? msg_at { get; set; }
        public int? msg_status { get; set; }

    }
}

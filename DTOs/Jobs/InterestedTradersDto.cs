namespace TradePlatform.Api.DTOs.Jobs
{

    public class review_reply_meta
    {
        public int? reply_id { get; set; }
        public string? reply_text { get; set; }
        public string? reply_at { get; set; }
    }

    public class review_request_meta: review_reply_meta
    {
        public bool? success { get; set; }
        public string? message { get; set; }
        public Guid? review_request_id { get; set; }
        public Guid? review_id { get; set; }
        public string? review_title { get; set; }
        public int? review_rating { get; set; }
        public string? review_text { get; set; }
        public DateTime? review_at { get; set; }
        public string? reviewer_first { get; set; }
        public string? reviewer_last { get; set; }
    }

    public class InterestedTradersDto: review_request_meta
    {
        
        public Guid job_purchase_id { get; set; }
        public Guid job_id { get; set; }
        public Guid trader_user_id { get; set; }        
        public string trader_first { get; set; }
        public string trader_last { get; set; }
        public string trader_phone { get; set; }
        public string business_name { get; set; }
        public string slug { get; set; }
        public string logoUrl { get; set; }

        public Guid? msg_id { get; set; }

        public string? msg_text { get; set; }

        public DateTime? msg_at { get; set; }
        public int? msg_status { get; set; }

    }
}

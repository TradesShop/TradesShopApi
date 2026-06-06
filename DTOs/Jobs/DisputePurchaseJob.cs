namespace TradePlatform.Api.DTOs.Jobs
{
    public class DisputePurchaseJob
    {
        public Guid id { get; set; }
        public Guid job_purchase_id { get; set; }
        public Guid job_id { get; set; }
        public Guid raised_by_user_id { get; set; }
        public Guid? against_user_id { get; set; }
        public int dispute_type { get; set; }
        public string reason { get; set; }
        public int status_id { get; set; }
        public string resolution_notes { get; set; }
        public int refund_credits { get; set; }
        public string decision { get; set; }
    }
    public class DisputeJobRequest
    {
    }
}

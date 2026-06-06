namespace TradePlatform.Api.DTOs.Stripe
{
    public class SubscriptionRequest
    {
        public Guid? targetuserid { get; set; } // Admin only
        public string priceid { get; set; }
        public string paymentmethodid { get; set; }
    }
}

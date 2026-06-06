namespace TradePlatform.Api.DTOs.Stripe
{
    public class SubscriptionDto
    {
        public Guid id { get; set; }
        public string stripe_subscriptionid { get; set; } = null!;
        public string stripe_priceid { get; set; } = null!;
        public string status { get; set; } = null!;
        public DateTime? periodstart { get; set; }
        public DateTime? periodend { get; set; }
    }
}

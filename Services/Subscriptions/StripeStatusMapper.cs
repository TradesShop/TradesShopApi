namespace TradePlatform.Api.Services.Subscriptions
{
    public class StripeStatusMapper
    {
        public static SubscriptionStatus MapStripeStatus(string? stripeStatus)
        {
            return stripeStatus switch
            {
                "trialing" => SubscriptionStatus.Trialing,
                "active" => SubscriptionStatus.Active,
                "past_due" => SubscriptionStatus.PastDue,
                "canceled" => SubscriptionStatus.Canceled,
                "unpaid" => SubscriptionStatus.Unpaid,
                "incomplete" => SubscriptionStatus.Incomplete,
                "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
                _ => SubscriptionStatus.Unknown
            };
        }
    }
}

namespace TradePlatform.Api.Services.Subscriptions
{
    public enum SubscriptionStatus
    {
        Unknown = 0,
        Trialing,
        Active,
        PastDue,
        Canceled,
        Unpaid,
        Incomplete,
        IncompleteExpired
    }
}

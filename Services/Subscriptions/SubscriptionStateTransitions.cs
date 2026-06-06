namespace TradePlatform.Api.Services.Subscriptions
{
    public class SubscriptionStateTransitions
    {
        private static readonly Dictionary<SubscriptionStatus, SubscriptionStatus[]> _allowed =
            new()
            {
                [SubscriptionStatus.Unknown] = new[]
                {
                    SubscriptionStatus.Trialing,
                    SubscriptionStatus.Active,
                    SubscriptionStatus.Incomplete
                },

                [SubscriptionStatus.Trialing] = new[]
                {
                    SubscriptionStatus.Active,
                    SubscriptionStatus.Canceled,
                    SubscriptionStatus.IncompleteExpired
                },

                [SubscriptionStatus.Active] = new[]
                {
                    SubscriptionStatus.PastDue,
                    SubscriptionStatus.Canceled,
                    SubscriptionStatus.Unpaid
                },

                [SubscriptionStatus.PastDue] = new[]
                {
                    SubscriptionStatus.Active,
                    SubscriptionStatus.Canceled,
                    SubscriptionStatus.Unpaid
                },

                [SubscriptionStatus.Unpaid] = new[]
                {
                    SubscriptionStatus.Canceled
                },

                [SubscriptionStatus.Incomplete] = new[]
                {
                    SubscriptionStatus.Active,
                    SubscriptionStatus.IncompleteExpired,
                    SubscriptionStatus.Canceled
                },

                [SubscriptionStatus.IncompleteExpired] = Array.Empty<SubscriptionStatus>(),
                [SubscriptionStatus.Canceled] = Array.Empty<SubscriptionStatus>()
            };

        public static bool CanTransition(SubscriptionStatus from, SubscriptionStatus to)
        {
            if (from == to) return true;
            return _allowed.TryGetValue(from, out var targets) && targets.Contains(to);
        }
    }
}

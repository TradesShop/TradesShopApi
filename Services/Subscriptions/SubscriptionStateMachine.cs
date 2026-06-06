namespace TradePlatform.Api.Services.Subscriptions
{
    public class SubscriptionStateMachine
    {
        public SubscriptionStatus Current { get; private set; }

        public SubscriptionStateMachine(SubscriptionStatus current)
        {
            Current = current;
        }

        public bool TryTransitionTo(SubscriptionStatus next, out string? reason)
        {
            if (SubscriptionStateTransitions.CanTransition(Current, next))
            {
                Current = next;
                reason = null;
                return true;
            }

            reason = $"Invalid transition: {Current} → {next}";
            return false;
        }
    }
}

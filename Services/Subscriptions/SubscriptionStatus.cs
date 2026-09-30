namespace TradePlatform.Api.Services.Subscriptions;

public enum SubscriptionStatus
{
	Unknown,
	Trialing,
	Active,
	CancelScheduled,
	PastDue,
	Canceled,
	Unpaid,
	Incomplete,
	IncompleteExpired
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace TradePlatform.Api.Services.Subscriptions;

public class SubscriptionStateTransitions
{
	private static readonly Dictionary<SubscriptionStatus, SubscriptionStatus[]> _allowed = new Dictionary<SubscriptionStatus, SubscriptionStatus[]>
	{
		[SubscriptionStatus.Unknown] = new SubscriptionStatus[3]
		{
			SubscriptionStatus.Trialing,
			SubscriptionStatus.Active,
			SubscriptionStatus.Incomplete
		},
		[SubscriptionStatus.Trialing] = new SubscriptionStatus[4]
		{
			SubscriptionStatus.Active,
			SubscriptionStatus.CancelScheduled,
			SubscriptionStatus.Canceled,
			SubscriptionStatus.IncompleteExpired
		},
		[SubscriptionStatus.Active] = new SubscriptionStatus[4]
		{
			SubscriptionStatus.CancelScheduled,
			SubscriptionStatus.PastDue,
			SubscriptionStatus.Canceled,
			SubscriptionStatus.Unpaid
		},
		[SubscriptionStatus.CancelScheduled] = new SubscriptionStatus[3]
		{
			SubscriptionStatus.Active,
			SubscriptionStatus.Canceled,
			SubscriptionStatus.PastDue
		},
		[SubscriptionStatus.PastDue] = new SubscriptionStatus[3]
		{
			SubscriptionStatus.Active,
			SubscriptionStatus.Canceled,
			SubscriptionStatus.Unpaid
		},
		[SubscriptionStatus.Unpaid] = new SubscriptionStatus[1] { SubscriptionStatus.Canceled },
		[SubscriptionStatus.Incomplete] = new SubscriptionStatus[3]
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
		if (from == to)
		{
			return true;
		}
		if (_allowed.TryGetValue(from, out SubscriptionStatus[] targets))
		{
			return Enumerable.Contains(targets, to);
		}
		return false;
	}
}

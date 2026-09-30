using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class CancelSubscriptionDto
{
	public string stripe_subscription_id { get; set; }

	public Guid? targetuserid { get; set; }
}

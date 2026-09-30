using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionCancelRequest
{
	public Guid? target_user_id { get; set; }

	public Guid plan_id { get; set; }

	public Guid plan_price_id { get; set; }

	public string stripe_subscription_id { get; set; }
}

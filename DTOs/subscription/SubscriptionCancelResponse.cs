using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionCancelResponse
{
	public Guid? subscription_id { get; set; }

	public string stripe_subscription_id { get; set; }

	public Guid plan_price_id { get; set; }

	public string status { get; set; }
}

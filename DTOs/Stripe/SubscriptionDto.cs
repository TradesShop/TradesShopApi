using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class SubscriptionDto
{
	public Guid id { get; set; }

	public string stripe_subscriptionid { get; set; }

	public string stripe_priceid { get; set; }

	public string status { get; set; }

	public DateTime? periodstart { get; set; }

	public DateTime? periodend { get; set; }
}

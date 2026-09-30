using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class SubscriptionRequest
{
	public Guid? targetuserid { get; set; }

	public string priceid { get; set; }

	public string paymentmethodid { get; set; }
}

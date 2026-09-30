using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionSelectResponse
{
	public string? message { get; set; } = string.Empty;

	public bool requires_payment_method { get; set; }

	public string? client_secret { get; set; }

	public bool ready_for_subscription { get; set; }

	public Guid? subscription_id { get; set; }

	public string stripe_subscription_id { get; set; }

	public bool is_scheduled { get; set; }

	public scheduled_response? scheduled { get; set; }

	public string? status { get; set; }
}

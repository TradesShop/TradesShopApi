using System;

namespace TradePlatform.Api.DTOs.subscription;

public class subscriptionpending_upsert
{
	public Guid user_id { get; set; }

	public Guid current_subscription_id { get; set; }

	public string stripe_subscription_id { get; set; }

	public Guid current_plan_price_id { get; set; }

	public Guid new_plan_price_id { get; set; }

	public string? current_stripe_price_id { get; set; }

	public string? new_stripe_price_id { get; set; }

	public string? stripe_schedule_id { get; set; }

	public DateTime? effective_date { get; set; }

	public string status { get; set; }

	public Guid? created_by { get; set; }

	public string? actor { get; set; }

	public string? source { get; set; }
}

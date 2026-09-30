using System;

namespace TradePlatform.Api.DTOs.subscription;

public class subscriptionpending_upd
{
	public Guid? pending_id { get; set; }

	public Guid? current_subscription_id { get; set; }

	public Guid? new_plan_price_id { get; set; }

	public string? new_stripe_price_id { get; set; }

	public string? stripe_schedule_id { get; set; }

	public DateTime? effective_date { get; set; }

	public string status { get; set; }

	public Guid? updated_by { get; set; }
}

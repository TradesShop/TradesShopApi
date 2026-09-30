using System;

namespace TradePlatform.Api.DTOs.subscription;

public class subscriptionpending_view
{
	public Guid pending_id { get; set; }

	public string receiver_email { get; set; }

	public string receiver_name { get; set; }

	public Guid user_id { get; set; }

	public Guid current_subscription_id { get; set; }

	public string stripe_subscription_id { get; set; }

	public Guid current_plan_price_id { get; set; }

	public Guid new_plan_price_id { get; set; }

	public DateTime? current_period_start { get; set; }

	public DateTime? current_period_end { get; set; }

	public string? current_stripe_price_id { get; set; }

	public string? new_stripe_price_id { get; set; }

	public string? stripe_schedule_id { get; set; }

	public DateTime effective_date { get; set; }

	public string status { get; set; }

	public string? created_by { get; set; }

	public string? current_plan_name { get; set; }

	public string? current_plan_type { get; set; }

	public decimal? current_plan_price { get; set; }

	public string? current_billing_interval { get; set; }

	public int? current_credits { get; set; }

	public bool current_is_recurring { get; set; }

	public string? new_plan_name { get; set; }

	public string? new_plan_type { get; set; }

	public decimal? new_plan_price { get; set; }

	public string? new_billing_interval { get; set; }

	public int? new_credits { get; set; }

	public bool new_is_recurring { get; set; }
}

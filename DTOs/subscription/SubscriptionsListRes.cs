using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionsListRes
{
	public Guid? plan_id { get; set; }

	public string? plan_name { get; set; }

	public string? plan_type { get; set; }

	public decimal? price { get; set; }

	public string? billing_interval { get; set; }

	public Guid? plan_price_id { get; set; }

	public Guid? subscription_id { get; set; }

	public Guid? user_id { get; set; }

	public string? status { get; set; }

	public bool auto_renew { get; set; }

	public bool cancel_at_period_end { get; set; }

	public DateTime? current_period_start { get; set; }

	public DateTime? current_period_end { get; set; }

	public DateTime? next_billing_date { get; set; }

	public DateTime? cancelled_at { get; set; }

	public DateTime? ended_at { get; set; }

	public DateTime? created_at { get; set; }

	public DateTime? updated_at { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionsHistoryDto
{
	public string? old_plan_type { get; set; }

	public string? new_plan_type { get; set; }

	public string? old_plan_name { get; set; }

	public string? new_plan_name { get; set; }

	public decimal? old_price { get; set; }

	public decimal? new_price { get; set; }

	public string? old_status { get; set; }

	public string? new_status { get; set; }

	public string? event_type { get; set; }

	public DateTime? old_current_period_end { get; set; }

	public DateTime? new_current_period_end { get; set; }

	public string? actor { get; set; }

	public string? source { get; set; }

	public Guid? id { get; set; }

	public Guid? subscription_id { get; set; }

	public Guid? user_id { get; set; }

	public DateTime? created_at { get; set; }

	public object? metadat { get; set; }
}

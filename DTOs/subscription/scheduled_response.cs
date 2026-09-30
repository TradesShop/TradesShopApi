using System;

namespace TradePlatform.Api.DTOs.subscription;

public class scheduled_response
{
	public string plan_type { get; set; }

	public string current_plan_name { get; set; }

	public decimal current_plan_price { get; set; }

	public string new_plan_name { get; set; }

	public decimal new_plan_price { get; set; }

	public DateTime effective_date { get; set; }
}

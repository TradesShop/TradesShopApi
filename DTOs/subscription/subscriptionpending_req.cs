using System;

namespace TradePlatform.Api.DTOs.subscription;

public class subscriptionpending_req
{
	public string search_mode { get; set; }

	public Guid? user_id { get; set; }

	public Guid? pending_id { get; set; }

	public string? stripe_schedule_id { get; set; }

	public Guid? current_subscription_id { get; set; }

	public string? plan_type { get; set; }
}

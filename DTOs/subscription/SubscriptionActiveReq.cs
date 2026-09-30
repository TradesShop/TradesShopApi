using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionActiveReq
{
	public Guid? target_user_id { get; set; }

	public string plan_type { get; set; }
}

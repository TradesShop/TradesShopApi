using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionsHistoryRes
{
	public IEnumerable<SubscriptionsHistoryDto>? history { get; set; }

	public int total_records { get; set; }
}

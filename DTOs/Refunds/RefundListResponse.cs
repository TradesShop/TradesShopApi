using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundListResponse
{
	public IEnumerable<RefundListItem>? items { get; set; }

	public int total_records { get; set; }
}

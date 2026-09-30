using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundRequestListResult
{
	public IEnumerable<RefundRequestListDto>? items { get; set; }

	public int total_records { get; set; }
}

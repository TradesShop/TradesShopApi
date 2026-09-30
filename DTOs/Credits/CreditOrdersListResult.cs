using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Credits;

public class CreditOrdersListResult
{
	public IEnumerable<CreditOrderItem>? items { get; set; }

	public int total_records { get; set; }
}

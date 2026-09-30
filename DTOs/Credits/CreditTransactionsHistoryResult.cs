using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Credits;

public class CreditTransactionsHistoryResult
{
	public IEnumerable<TransactionItem>? items { get; set; }

	public int total_records { get; set; }
}

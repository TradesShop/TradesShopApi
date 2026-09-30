using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Invoices;

public class InvoiceListResponse
{
	public IEnumerable<InvoiceListItem>? items { get; set; }

	public int total_records { get; set; }
}

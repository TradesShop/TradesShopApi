using System;

namespace TradePlatform.Api.DTOs.Invoices;

public class InvoiceListItem
{
	public Guid id { get; set; }

	public string invoice_number { get; set; }

	public string type { get; set; }

	public string plan_type { get; set; }

	public decimal net_amount { get; set; }

	public decimal tax_amount { get; set; }

	public decimal gross_amount { get; set; }

	public string status { get; set; }

	public DateTime? paid_at { get; set; }

	public string invoice_url { get; set; }

	public DateTime created_at { get; set; }
}

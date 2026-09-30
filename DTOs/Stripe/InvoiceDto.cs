using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class InvoiceDto
{
	public string id { get; set; }

	public string status { get; set; }

	public long amount_due { get; set; }

	public long amount_paid { get; set; }

	public long amount_remaining { get; set; }

	public string hosted_invoice_url { get; set; }

	public string invoice_pdf { get; set; }

	public DateTime created_at { get; set; }
}

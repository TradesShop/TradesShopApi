using System;

namespace TradePlatform.Api.DTOs.Invoices;

public class InvEventEmailNotifyDto
{
	public string stripe_invoice_id { get; set; }

	public Guid user_id { get; set; }
}

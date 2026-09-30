using System;
using System.Collections.Generic;
using TradePlatform.Api.DTOs.Invoices;

namespace TradePlatform.Api.DTOs.Payments;

public class StartPaymentRequestDto
{
	public Guid UserId { get; set; }

	public string Currency { get; set; }

	public string BillingEmail { get; set; }

	public string Type { get; set; }

	public List<InvoiceItemCreateDto> Items { get; set; } = new List<InvoiceItemCreateDto>();
}

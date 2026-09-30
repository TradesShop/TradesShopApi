using System;

namespace TradePlatform.Api.DTOs.Invoices;

public class RefundRequestDto
{
	public Guid PaymentId { get; set; }

	public decimal Amount { get; set; }

	public string Reason { get; set; }
}

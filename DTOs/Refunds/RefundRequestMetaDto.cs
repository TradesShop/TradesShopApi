using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundRequestMetaDto
{
	public Guid refund_request_id { get; set; }

	public string payment_intent_id { get; set; }

	public Guid invoice_id { get; set; }

	public decimal refund_amount { get; set; }

	public DateTime created_at { get; set; }
}

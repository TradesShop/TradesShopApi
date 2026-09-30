using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundCalculationResponse
{
	public DateTime? period_start_date { get; set; }

	public DateTime? period_end_date { get; set; }

	public int? total_days { get; set; }

	public int? used_days { get; set; }

	public int? remaining_days { get; set; }

	public int? total_credits { get; set; }

	public int? remaining_credits { get; set; }

	public decimal paid_net { get; set; }

	public decimal paid_vat { get; set; }

	public decimal paid_gross { get; set; }

	public Guid invoice_id { get; set; }

	public string? payment_intent_id { get; set; }

	public decimal refund_net { get; set; }

	public decimal refund_vat { get; set; }

	public decimal refund_gross { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundListItem
{
	public Guid id { get; set; }

	public string refund_number { get; set; }

	public string plan_type { get; set; }

	public string entity_type { get; set; }

	public decimal refund_net { get; set; }

	public decimal refund_vat { get; set; }

	public decimal refund_gross { get; set; }

	public string currency { get; set; }

	public string status { get; set; }

	public string stripe_refund_status { get; set; }

	public string stripe_refund_reason { get; set; }

	public string stripe_failure_reason { get; set; }

	public string notes { get; set; }

	public DateTime requested_at { get; set; }

	public DateTime? approved_at { get; set; }

	public DateTime? processed_at { get; set; }

	public DateTime created_at { get; set; }

	public string invoice_url { get; set; }
}

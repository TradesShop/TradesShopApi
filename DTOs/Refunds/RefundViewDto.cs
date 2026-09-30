using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundViewDto
{
	public Guid id { get; set; }

	public Guid refund_request_id { get; set; }

	public Guid invoice_id { get; set; }

	public Guid user_uid { get; set; }

	public int entity_type_id { get; set; }

	public Guid entity_id { get; set; }

	public string payment_intent_id { get; set; }

	public string? stripe_charge_id { get; set; }

	public string? stripe_refund_id { get; set; }

	public string? stripe_refund_reason { get; set; }

	public decimal refund_net { get; set; }

	public decimal refund_vat { get; set; }

	public decimal refund_gross { get; set; }

	public string currency { get; set; }

	public int status_id { get; set; }

	public string? stripe_refund_status { get; set; }

	public string? notes { get; set; }

	public long requested_by { get; set; }

	public long? approved_by { get; set; }

	public DateTime requested_at { get; set; }

	public DateTime? approved_at { get; set; }

	public DateTime? processed_at { get; set; }

	public string? stripe_failure_reason { get; set; }

	public int refund_req_status_id { get; set; }

	public string refund_req_status { get; set; }

	public DateTime created_at { get; set; }

	public DateTime? updated_at { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundCreateRequest
{
	public Guid refund_request_id { get; set; }

	public decimal? refund_net { get; set; }

	public decimal? refund_vat { get; set; }

	public decimal? refund_gross { get; set; }

	public string currency { get; set; } = "gbp";

	public int? refund_req_status_id { get; set; }

	public string? stripe_refund_reason { get; set; }

	public string? notes { get; set; }

	public bool? send__email { get; set; }

	public Guid updated_by { get; set; }
}

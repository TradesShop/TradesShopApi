using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundUpdateRequest
{
	public Guid refund_request_id { get; set; }

	public Guid refund_id { get; set; }

	public int status_id { get; set; }

	public string stripe_refund_id { get; set; }

	public string stripe_failure_reason { get; set; }

	public string stripe_refund_status { get; set; }

	public string comments { get; set; }

	public Guid updated_by { get; set; }

	public string? receipt_url { get; set; }
}

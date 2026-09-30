using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class DisputeActionReq
{
	public Guid id { get; set; }

	public int? priority_id { get; set; }

	public int? status_id { get; set; }

	public int? decision_id { get; set; }

	public string? resolution_notes { get; set; }

	public int? refund_credits { get; set; }

	public Guid? user_id { get; set; }

	public bool send_email { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class DisputeJobResponse
{
	public Guid job_dispute_id { get; set; }

	public Guid job_purchase_id { get; set; }

	public Guid job_id { get; set; }

	public string job_ref { get; set; }

	public string job_title { get; set; }

	public DateTime job_created_at { get; set; }

	public Guid receiver_id { get; set; }

	public string receiver_name { get; set; }

	public string receiver_email { get; set; }

	public string lastname { get; set; }

	public bool receive_important { get; set; } = true;

	public string status { get; set; }

	public string status_code { get; set; }

	public int status_id { get; set; }

	public int dispute_type_id { get; set; }

	public string? dispute_type { get; set; }

	public string? reason { get; set; }

	public DateTime dispute_created_at { get; set; }

	public int purchased_credits { get; set; }

	public int? priority_id { get; set; }

	public int? decision_id { get; set; }

	public string? dispute_decision { get; set; }

	public string? resolution_notes { get; set; }

	public int refund_credits { get; set; }
}

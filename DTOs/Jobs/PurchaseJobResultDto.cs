using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class PurchaseJobResultDto
{
	public bool success { get; set; }

	public string title { get; set; }

	public string message { get; set; }

	public Guid job_purchase_id { get; set; }

	public Guid job_id { get; set; }

	public bool is_customer { get; set; }
}

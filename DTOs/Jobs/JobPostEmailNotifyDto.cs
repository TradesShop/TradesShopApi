using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobPostEmailNotifyDto
{
	public Guid user_id { get; set; }

	public Guid job_id { get; set; }

	public string customer_email { get; set; }

	public string customer_name { get; set; }

	public bool is_customer { get; set; }
}

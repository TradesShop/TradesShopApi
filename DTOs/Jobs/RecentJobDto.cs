using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class RecentJobDto
{
	public Guid trader_id { get; set; }

	public Guid job_id { get; set; }

	public string job_url { get; set; }

	public string title { get; set; }

	public string description { get; set; }

	public string category_name { get; set; }

	public DateTime created_at { get; set; }
}

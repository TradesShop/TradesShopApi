using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class MyJobsResponseDto
{
	public Guid id { get; set; }

	public string job_ref { get; set; }

	public string category { get; set; }

	public int int_id { get; set; }

	public string title { get; set; }

	public string status { get; set; }

	public string status_code { get; set; }

	public DateTime created_at { get; set; }
}

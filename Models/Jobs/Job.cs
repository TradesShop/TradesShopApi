using System;

namespace TradePlatform.Api.Models.Jobs;

public class Job
{
	public Guid id { get; set; }

	public long uid { get; set; }

	public Guid? user_id { get; set; }

	public string? job_ref { get; set; }

	public string? phone { get; set; }

	public string? email { get; set; }

	public string? category { get; set; }

	public string title { get; set; }

	public string description { get; set; }

	public int credit_cost { get; set; }

	public string postcode { get; set; }

	public int status_id { get; set; }

	public decimal distance_km { get; set; }

	public DateTime created_at { get; set; }
}

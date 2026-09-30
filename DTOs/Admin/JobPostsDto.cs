using System;

namespace TradePlatform.Api.DTOs.Admin;

public class JobPostsDto
{
	public Guid id { get; set; }

	public string job_ref { get; set; }

	public int credit_cost { get; set; }

	public int? status_id { get; set; }

	public string? status { get; set; }

	public string? category { get; set; }

	public string? title { get; set; }

	public string description { get; set; }

	public string postcode { get; set; }

	public DateTime created_at { get; set; }

	public object? workplace { get; set; }

	public int budget_range_id { get; set; }

	public string budget_range { get; set; }

	public int purchase_limit { get; set; }

	public int purchased_count { get; set; }

	public int exists_count { get; set; }

	public string? status_tags { get; set; }
}

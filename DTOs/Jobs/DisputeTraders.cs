using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class DisputeTraders
{
	public Guid job_dispute_id { get; set; }

	public Guid job_purchase_id { get; set; }

	public Guid trader_id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string email { get; set; }

	public string phone { get; set; }

	public int user_type { get; set; }

	public DateTime created_at { get; set; }

	public Guid? against_user_id { get; set; }

	public int dispute_type_id { get; set; }

	public int? status_id { get; set; }

	public string? status { get; set; }

	public string? business_name { get; set; }

	public string? slug { get; set; }

	public string? logoUrl { get; set; }

	public decimal? rating { get; set; }
}

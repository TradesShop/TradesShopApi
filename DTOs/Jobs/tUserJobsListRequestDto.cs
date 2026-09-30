using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class tUserJobsListRequestDto
{
	public Guid? target_user_id { get; set; }

	public Guid? user_id { get; set; }

	public int status_id { get; set; }

	public DateTime? last_created_at { get; set; }

	public int? last_id { get; set; }

	public int? limit { get; set; }

	public string? sort_key { get; set; }

	public string? sort_dir { get; set; }

	public string? category_ids { get; set; }

	public string? location_ids { get; set; }

	public int? max_credits { get; set; }

	public decimal? max_distance { get; set; }

	public int? last_credit { get; set; }

	public decimal? last_distance { get; set; }
}

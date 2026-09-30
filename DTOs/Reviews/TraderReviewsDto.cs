using System;

namespace TradePlatform.Api.DTOs.Reviews;

public class TraderReviewsDto
{
	public string? slug { get; set; }

	public Guid? user_id { get; set; }

	public string sort_by { get; set; }

	public int page_number { get; set; }

	public int page_size { get; set; }
}

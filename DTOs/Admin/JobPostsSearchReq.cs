using System;

namespace TradePlatform.Api.DTOs.Admin;

public class JobPostsSearchReq
{
	public string? search_text { get; set; }

	public string? postcode { get; set; }

	public int? category_id { get; set; }

	public int? status_id { get; set; }

	public string? search_from { get; set; }

	public string? search_to { get; set; }

	public Guid? target_user_id { get; set; }

	public int page_number { get; set; }

	public int page_size { get; set; }
}

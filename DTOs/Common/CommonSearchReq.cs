using System;

namespace TradePlatform.Api.DTOs.Common;

public class CommonSearchReq
{
	public Guid? target_user_id { get; set; }

	public string? search { get; set; }

	public string? sort_by { get; set; }

	public int? page_number { get; set; }

	public int? page_size { get; set; }
}

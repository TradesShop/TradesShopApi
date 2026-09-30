using System;

namespace TradePlatform.Api.DTOs.Credits;

public class credit_history_request
{
	public Guid? user_id { get; set; }

	public Guid? target_user_id { get; set; }

	public int page_number { get; set; } = 1;

	public int page_size { get; set; } = 20;
}

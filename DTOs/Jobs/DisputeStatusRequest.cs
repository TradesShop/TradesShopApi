using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class DisputeStatusRequest
{
	public Guid? id { get; set; }

	public string status_code { get; set; }

	public Guid? user_id { get; set; }

	public Guid? target_user_id { get; set; }

	public bool is_admin { get; set; }
}

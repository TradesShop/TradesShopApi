using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class DisputeJobReq
{
	public Guid dispute_id { get; set; }

	public Guid? user_id { get; set; }

	public Guid? target_user_id { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobLocationMeta
{
	public Guid job_id { get; set; }

	public Guid? user_id { get; set; }
}

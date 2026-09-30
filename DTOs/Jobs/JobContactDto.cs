using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobContactDto : JobContactBase
{
	public Guid contact_id { get; set; }

	public Guid job_id { get; set; }

	public Guid? user_id { get; set; }
}

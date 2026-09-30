using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobAssignTradeRequest
{
	public Guid job_id { get; set; }

	public Guid job_purchase_id { get; set; }

	public Guid? user_id { get; set; }
}

using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobUpdateStatus : JobUpdateBase
{
	public string statuscode { get; set; }

	public Guid? completed_by { get; set; }

	public int? closure_reason_id { get; set; }

	public string? note { get; set; }
}

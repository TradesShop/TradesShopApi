using System;

namespace TradePlatform.Api.Models.Jobs;

public class Job_meta
{
	public Guid id { get; set; }

	public Guid user_id { get; set; }

	public string title { get; set; }

	public int status_id { get; set; }

	public string description { get; set; }

	public long job_assigned_to { get; set; }

	public DateTime job_assigned_at { get; set; }
}

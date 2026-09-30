using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Jobs;

public class TraderDto
{
	public Guid trader_id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string email { get; set; }

	public string phone { get; set; }

	public string category_name { get; set; }

	public PostedJobDto PostedJob { get; set; }

	public List<RecentJobDto> RecentJobs { get; set; } = new List<RecentJobDto>();
}

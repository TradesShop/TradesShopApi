using System.Collections.Generic;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.Services.Emails.Models;

public class JobPostedTraderNotifyModel : EmailModelBase
{
	public string TraderName { get; set; }

	public PostedJobDto PostedJob { get; set; }

	public List<RecentJobDto> RecentJobs { get; set; } = new List<RecentJobDto>();
}

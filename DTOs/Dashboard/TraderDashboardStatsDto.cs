namespace TradePlatform.Api.DTOs.Dashboard;

public class TraderDashboardStatsDto
{
	public int recommended_jobs_count { get; set; }

	public int purchased_jobs_count { get; set; }

	public decimal overall_rating { get; set; }

	public int total_reviews { get; set; }
}

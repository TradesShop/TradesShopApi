namespace TradePlatform.Api.DTOs.Jobs;

public class JobUpdateDto : JobUpdateBase
{
	public string? action { get; set; }

	public string? title { get; set; }

	public string? description { get; set; }

	public int? status_id { get; set; }

	public int? budget_range_id { get; set; }

	public int? timeline_id { get; set; }

	public int? credit_cost { get; set; }

	public int? purchase_limit { get; set; }
}

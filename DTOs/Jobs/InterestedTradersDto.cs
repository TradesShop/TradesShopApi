using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class InterestedTradersDto : review_request_meta
{
	public Guid job_purchase_id { get; set; }

	public Guid job_id { get; set; }

	public Guid trader_user_id { get; set; }

	public string trader_first { get; set; }

	public string trader_last { get; set; }

	public string trader_phone { get; set; }

	public string business_name { get; set; }

	public string slug { get; set; }

	public string logoUrl { get; set; }

	public Guid? msg_id { get; set; }

	public string? msg_text { get; set; }

	public DateTime? msg_at { get; set; }

	public int? msg_status { get; set; }

	public decimal overall_rating { get; set; }

	public int total_reviews { get; set; }

	public int status_id { get; set; }

	public string status_code { get; set; }
}

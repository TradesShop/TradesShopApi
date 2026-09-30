using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobPurchaseNotifyDto
{
	public Guid job_purchase_id { get; set; }

	public Guid job_owner_id { get; set; }

	public Guid job_buyer_id { get; set; }

	public Guid job_id { get; set; }

	public string customer_name { get; set; }

	public string customer_email { get; set; }

	public string customer_phone { get; set; }

	public string trader_name { get; set; }

	public string trader_email { get; set; }

	public string trader_phone { get; set; }

	public string job_title { get; set; }

	public string job_description { get; set; }

	public string business_name { get; set; }

	public string postcode { get; set; }

	public string slug { get; set; }

	public DateTime job_purchased_at { get; set; }

	public DateTime job_created_at { get; set; }
}

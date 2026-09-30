using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobFullDetailsDto
{
	public string job_ref { get; set; }

	public Guid id { get; set; }

	public string title { get; set; }

	public string category { get; set; }

	public Guid owner_id { get; set; }

	public string categoryname { get; set; }

	public string description { get; set; }

	public string location { get; set; }

	public decimal? latitude { get; set; }

	public decimal? longitude { get; set; }

	public int credit_cost { get; set; }

	public int distance_km { get; set; }

	public Guid? job_purchase_id { get; set; }

	public bool has_messaged_before { get; set; }

	public DateTime created_at { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string email { get; set; }

	public string phone { get; set; }

	public string postcode { get; set; }

	public string country_code { get; set; }

	public string jpostcode { get; set; }

	public string address_line1 { get; set; }

	public string address_line2 { get; set; }

	public string city { get; set; }

	public int status_id { get; set; }

	public string status_code { get; set; }

	public Guid contact_id { get; set; }

	public int budget_range_id { get; set; }

	public int timeline_id { get; set; }

	public Guid? job_dispute_id { get; set; }

	public List<JobQuestionDto> Questions { get; set; }

	public int purchase_limit { get; set; }

	public int purchased_count { get; set; }

	public object workplace { get; set; }
}

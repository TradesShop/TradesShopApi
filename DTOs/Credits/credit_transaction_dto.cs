using System;

namespace TradePlatform.Api.DTOs.Credits;

public class credit_transaction_dto
{
	public string label { get; set; }

	public int credits { get; set; }

	public string description { get; set; }

	public string reason { get; set; }

	public string job_title { get; set; }

	public string addon_name { get; set; }

	public DateTime created_at { get; set; }

	public Guid id { get; set; }
}

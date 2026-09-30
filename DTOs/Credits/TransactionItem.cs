using System;

namespace TradePlatform.Api.DTOs.Credits;

public class TransactionItem
{
	public Guid id { get; set; }

	public string? grant_ids { get; set; }

	public string job_ref { get; set; }

	public string type { get; set; }

	public int credits { get; set; }

	public string entity_type { get; set; }

	public Guid entity_id { get; set; }

	public string? metadata { get; set; }

	public DateTime created_at { get; set; }
}

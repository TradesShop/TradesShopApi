using System;

namespace TradePlatform.Api.DTOs.Credits;

public class CreditGrantRequest
{
	public Guid user_id { get; set; }

	public string source { get; set; } = string.Empty;

	public Guid? reference_id { get; set; }

	public int total_credits { get; set; }

	public DateTime expires_at { get; set; }

	public string? reference_type { get; set; }

	public string? metadata { get; set; }
}

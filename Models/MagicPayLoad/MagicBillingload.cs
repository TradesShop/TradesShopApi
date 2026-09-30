using System;

namespace TradePlatform.Api.Models.MagicPayLoad;

public class MagicBillingload
{
	public Guid user_id { get; set; }

	public Guid entity_id { get; set; }

	public int? entity_type_id { get; set; }

	public string redirect { get; set; }

	public DateTime expires_at { get; set; }
}

using System;

namespace TradePlatform.Api.Models;

public class UserBasic
{
	public Guid id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string email { get; set; }

	public string phone { get; set; }

	public int? user_type { get; set; }

	public bool isactive { get; set; }

	public DateTime created_at { get; set; }
}

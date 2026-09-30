using System;

namespace TradePlatform.Api.DTOs.users;

public class TradeUsersDto
{
	public Guid user_id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string email { get; set; }

	public int credits { get; set; }

	public string membership { get; set; }

	public string? business { get; set; }

	public string? slug { get; set; }
}

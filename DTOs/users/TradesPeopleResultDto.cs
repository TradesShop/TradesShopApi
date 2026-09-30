using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.users;

public class TradesPeopleResultDto
{
	public IEnumerable<TradeUsersDto>? traders { get; set; }

	public int total_records { get; set; }
}

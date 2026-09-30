using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Reviews;

public class TraderReviewsResultDto
{
	public IEnumerable<ReviewsDto>? reviews { get; set; }

	public int total_records { get; set; }
}

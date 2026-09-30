using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Admin;

public class JobPostsSearchResult
{
	public IEnumerable<JobPostsDto>? job_posts { get; set; }

	public int total_records { get; set; }
}

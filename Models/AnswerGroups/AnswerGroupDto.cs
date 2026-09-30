using System;

namespace TradePlatform.Api.Models.AnswerGroups;

public class AnswerGroupDto
{
	public int id { get; set; }

	public int category_id { get; set; }

	public string name { get; set; }

	public string category { get; set; }

	public DateTime? created_at { get; set; }
}

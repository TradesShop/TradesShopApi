using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Business;

public class BusinessSkillsUpdateDto
{
	public Guid? id { get; set; }

	public Guid? business_id { get; set; }

	public int category_id { get; set; }

	public List<int> skills_ids { get; set; } = new List<int>();
}

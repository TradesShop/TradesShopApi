using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Business;

public class BusinessCategorySkillResponseDto
{
	public Guid? id { get; set; }

	public Guid business_id { get; set; }

	public int category_id { get; set; }

	public string category_name { get; set; }

	public bool is_primary { get; set; }

	public List<SkillDto> skills { get; set; } = new List<SkillDto>();
}

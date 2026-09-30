using System;

namespace TradePlatform.Api.DTOs.Business;

public class BusinessCategorySkillFlatDto
{
	public Guid business_id { get; set; }

	public int category_id { get; set; }

	public string category_name { get; set; }

	public bool is_primary { get; set; }

	public int category_skill_id { get; set; }

	public string category_skill_name { get; set; }
}

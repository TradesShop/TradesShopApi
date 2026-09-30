namespace TradePlatform.Api.Models;

public class categories : category
{
	public bool is_default { get; set; }

	public bool is_active { get; set; }

	public int category_type_id { get; set; }

	public string? category_type { get; set; }

	public int sortorder { get; set; }

	public string description { get; set; }

	public int? parent_id { get; set; }
}

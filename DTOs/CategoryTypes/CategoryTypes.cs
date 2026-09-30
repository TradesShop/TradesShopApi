using System;

namespace TradePlatform.Api.DTOs.CategoryTypes;

public class CategoryTypes
{
	public int id { get; set; }

	public string name { get; set; }

	public string code { get; set; }

	public string description { get; set; }

	public int sort_order { get; set; }

	public bool is_active { get; set; }

	public DateTime created_at { get; set; }
}

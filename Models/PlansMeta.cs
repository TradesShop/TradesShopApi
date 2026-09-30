using System;

namespace TradePlatform.Api.Models;

public class PlansMeta
{
	public Guid? id { get; set; }

	public string code { get; set; }

	public string name { get; set; }

	public int trade_categories { get; set; }

	public bool is_active { get; set; } = true;

	public int sort_order { get; set; }

	public string type { get; set; }

	public bool is_vatable { get; set; } = true;

	public bool is_highlighted { get; set; }

	public string? highlight_label { get; set; }

	public int expiry_months { get; set; }

	public DateTime? created_at { get; set; }

	public DateTime? updated_at { get; set; }
}

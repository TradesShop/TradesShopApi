using System;

namespace TradePlatform.Api.Models.PlanPrices;

public class PlanPricesMeta
{
	public Guid? id { get; set; }

	public Guid plan_id { get; set; }

	public decimal price { get; set; }

	public string currency { get; set; }

	public string billing_interval { get; set; }

	public string? stripe_price_id { get; set; }

	public string? stripe_product_id { get; set; }

	public int credits_per_period { get; set; }

	public int extra_categories { get; set; }

	public int extra_locations { get; set; }

	public int boost_score { get; set; }

	public bool is_recurring { get; set; }

	public bool is_active { get; set; }

	public DateTime? valid_from { get; set; }

	public DateTime? valid_to { get; set; }

	public string description { get; set; }

	public DateTime? created_at { get; set; }
}

using System;

namespace TradePlatform.Api.Models.Plans;

public class PlanPriceByPriceId
{
	public Guid plan_id { get; set; }

	public Guid plan_price_id { get; set; }

	public string plan_type { get; set; }

	public string name { get; set; }

	public decimal price { get; set; }

	public string currency { get; set; }

	public string billing_interval { get; set; }

	public bool is_recurring { get; set; }

	public bool is_active { get; set; }

	public string stripe_price_id { get; set; }

	public string stripe_product_id { get; set; }

	public int credits_per_period { get; set; }
}

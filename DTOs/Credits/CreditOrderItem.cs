using System;

namespace TradePlatform.Api.DTOs.Credits;

public class CreditOrderItem
{
	public Guid id { get; set; }

	public string stripe_payment_intent_id { get; set; }

	public Guid plan_id { get; set; }

	public Guid plan_price_id { get; set; }

	public Guid invoice_id { get; set; }

	public string invoice_number { get; set; }

	public string type { get; set; }

	public string plan_type { get; set; }

	public decimal net_amount { get; set; }

	public decimal tax_amount { get; set; }

	public decimal gross_amount { get; set; }

	public string currency { get; set; }

	public string status { get; set; }

	public string invoice_url { get; set; }

	public DateTime paid_at { get; set; }

	public DateTime created_at { get; set; }

	public int total_credits { get; set; }

	public int remaining_credits { get; set; }
}

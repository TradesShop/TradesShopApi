using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class PaymentMethodDto
{
	public Guid? id { get; set; }

	public string? name_on_card { get; set; }

	public string brand { get; set; }

	public string last4 { get; set; }

	public int exp_month { get; set; }

	public int exp_year { get; set; }

	public bool is_default { get; set; }
}

using System;

namespace TradePlatform.Api.Models.BundleCredit;

public class BundlePurchaseResponse
{
	public bool success { get; set; }

	public string? message { get; set; }

	public string? client_secret { get; set; }

	public Guid? bundle_order_id { get; set; }

	public bool requires_action { get; set; }

	public bool requires_payment_method { get; set; }
}

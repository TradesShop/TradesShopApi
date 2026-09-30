using System;
using System.Collections.Generic;
using TradePlatform.Api.DTOs.Invoices;

namespace TradePlatform.Api.DTOs.Bundles;

public class BundlePurchaseCompletedDto
{
	public Guid bundle_order_id { get; set; }

	public Guid bundle_price_id { get; set; }

	public Guid user_id { get; set; }

	public string stripe_payment_intent_id { get; set; }

	public string stripe_charge_id { get; set; }

	public string status { get; set; }

	public string billing_email { get; set; }

	public decimal amount_total { get; set; }

	public decimal amount_subtotal { get; set; }

	public decimal amount_vat { get; set; }

	public string currency { get; set; }

	public string stripe_invoice_id { get; set; }

	public string stripe_event_id { get; set; }

	public int entity_type_id { get; set; }

	public Guid entity_id { get; set; }

	public string entity_type { get; set; }

	public string invoice_url { get; set; }

	public DateTime? paid_at { get; set; }

	public List<InvoiceItemCreateDto> Items { get; set; } = new List<InvoiceItemCreateDto>();

	public string metadataJson { get; set; }
}

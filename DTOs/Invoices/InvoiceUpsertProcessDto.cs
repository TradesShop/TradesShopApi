using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Invoices;

public class InvoiceUpsertProcessDto
{
	public string stripe_invoice_id { get; set; }

	public string stripe_invoice_number { get; set; }

	public string stripe_receipt_number { get; set; }

	public Guid entity_id { get; set; }

	public int entity_type_id { get; set; }

	public string entity_type { get; set; }

	public Guid user_id { get; set; }

	public Guid plan_price_id { get; set; }
    public string? stripe_price_id { get; set; }

    public string status { get; set; }

	public string currency { get; set; }

	public decimal? amount_subtotal { get; set; }

	public decimal? amount_vat { get; set; }

	public decimal? amount_discount { get; set; }

	public decimal? amount_total { get; set; }

	public string? billing_email { get; set; }

	public string? stripe_payment_intent_id { get; set; }

	public string? stripe_charge_id { get; set; }

	public string stripe_event_id { get; set; }

	public string? invoice_url { get; set; }

	public string? metadata_json { get; set; }

	public DateTime? paid_at { get; set; }

	public DateTime? billing_period_start { get; set; }

	public DateTime? billing_period_end { get; set; }

	public string invoice_type { get; set; }

	public List<InvoiceItemCreateDto>? Items { get; set; } = new List<InvoiceItemCreateDto>();
}

namespace TradePlatform.Api.DTOs.Invoices
{
    public class InvoiceEventProcessDto
    {
        public Guid user_id { get; set; }
        public Guid plan_price_id { get; set; }
        public Guid subscription_id { get; set; }
        public string stripe_invoice_id { get; set; }
        public string stripe_payment_intent_id { get; set; }
        public string status { get; set; }
        public string invoice_type { get; set; }
        public string currency { get; set; }
        public decimal subtotal { get; set; }
        public decimal tax_amount { get; set; }        
        public decimal discount_amount { get; set; }
        public decimal total_amount { get; set; }
        public string billing_email { get; set; }
        public DateTime? billing_period_start { get; set; }
        public DateTime? billing_period_end { get; set; }
        public DateTime? issued_at { get; set; }
        public DateTime? paid_at { get; set; }
        public DateTime? due_at { get; set; }
        public string metadata_json { get; set; }
        public string stripe_event_id { get; set; }
        public string actor { get; set; }
        public string? source { get; set; }
        public string event_type { get; set; }
        public string pdf_url { get; set; }
        public List<InvoiceItemCreateDto> Items { get; set; } = new();
    }
}

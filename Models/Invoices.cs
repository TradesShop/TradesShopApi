namespace TradePlatform.Api.Models
{
    public class Invoices
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string invoice_number { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
        public decimal subtotal { get; set; }
        public decimal tax_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal total_amount { get; set; }
        public string stripe_invoice_id { get; set; }
        public string stripe_payment_intent_id { get; set; }
        public string stripe_customer_id { get; set; }
        public string billing_email { get; set; }
        public DateTime issued_at { get; set; }
        public DateTime? paid_at { get; set; }
        public DateTime? due_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class InvoiceItems
    {
        public Guid invoice_id { get; set; }
        public string entity_type { get; set; }
        public Guid? entity_id { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal total_price { get; set; }
        public string? metadata { get; set; }
        public DateTime created_at { get; set; }
    }
}

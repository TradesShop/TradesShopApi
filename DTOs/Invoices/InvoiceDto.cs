namespace TradePlatform.Api.DTOs.Invoices
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string StripeInvoiceId { get; set; }
        public string StripePaymentIntentId { get; set; }
        public string StripeCustomerId { get; set; }
        public string BillingEmail { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<InvoiceItemDto> Items { get; set; } = new();
    }
}

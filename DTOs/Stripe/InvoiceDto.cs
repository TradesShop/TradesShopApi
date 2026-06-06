namespace TradePlatform.Api.DTOs.Stripe
{
    public class InvoiceDto
    {
        public string id { get; set; } = null!;
        public string status { get; set; } = null!;
        public long amount_due { get; set; }
        public long amount_paid { get; set; }
        public long amount_remaining { get; set; }
        public string hosted_invoice_url { get; set; } = null!;
        public string invoice_pdf { get; set; } = null!;
        public DateTime created_at { get; set; }
    }
}

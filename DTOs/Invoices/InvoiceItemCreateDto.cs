namespace TradePlatform.Api.DTOs.Invoices
{
    public class InvoiceItemCreateDto
    {
        public string entity_type { get; set; }
        public Guid? entity_id { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal total_price { get; set; }
        public string Metadata { get; set; }
    }
}

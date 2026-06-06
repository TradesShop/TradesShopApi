namespace TradePlatform.Api.DTOs.Invoices
{
    public class InvoiceItemDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

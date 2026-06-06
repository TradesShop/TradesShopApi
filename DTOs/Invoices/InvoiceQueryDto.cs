namespace TradePlatform.Api.DTOs.Invoices
{
    public class InvoiceQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? Status { get; set; }
        public Guid? UserId { get; set; }
        public string? InvoiceNumber { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string SortBy { get; set; } = "created_at";
        public string SortDirection { get; set; } = "DESC";
    }
}

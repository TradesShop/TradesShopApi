namespace TradePlatform.Api.DTOs.Invoices
{
    public class RefundRequestDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; } // partial or full
        public string Reason { get; set; }  // optional
    }
}

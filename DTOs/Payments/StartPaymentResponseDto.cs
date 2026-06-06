namespace TradePlatform.Api.DTOs.Payments
{
    public class StartPaymentResponseDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string StripePaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
    }
}

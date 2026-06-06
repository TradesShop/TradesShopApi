namespace TradePlatform.Api.DTOs.Payments
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid InvoiceId { get; set; }
        public string StripePaymentIntentId { get; set; }
        public string StripeChargeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

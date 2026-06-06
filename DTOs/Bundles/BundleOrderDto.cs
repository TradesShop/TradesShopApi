namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundleOrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid Bundle_Price_Id { get; set; }
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? StripePriceId { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.Stripe
{
    public class SubscribeRequestDto
    {
        public string price_id { get; set; } = null!;
        public string payment_method_id { get; set; } = null!;
    }
}

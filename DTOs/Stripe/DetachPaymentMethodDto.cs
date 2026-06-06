namespace TradePlatform.Api.DTOs.Stripe
{
    public class DetachPaymentMethodDto
    {
        public Guid? target_user_id { get; set; }// Admin only
        public string? stripe_payment_method_id { get; set; } 
    }
}

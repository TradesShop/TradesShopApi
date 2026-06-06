namespace TradePlatform.Api.DTOs.Stripe
{
    public class SetupIntentDto
    {
        public Guid? target_user_id { get; set; } // Admin only     
    }
}

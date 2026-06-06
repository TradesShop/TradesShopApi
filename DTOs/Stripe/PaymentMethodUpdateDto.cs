namespace TradePlatform.Api.DTOs.Stripe
{
    public class PaymentMethodUpdateDto
    {
        public string? name_on_card { get; set; }
        public int exp_month { get; set; }
        public int exp_year { get; set; }
        public Guid? target_user_id { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.Stripe;

public class AddPaymentMethodRequestDto
{
	public string payment_method_id { get; set; }

	public bool make_primary { get; set; } = true;
}

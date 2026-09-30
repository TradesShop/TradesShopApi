using System;

namespace TradePlatform.Api.DTOs.Stripe;

public class AttachPaymentMethodDto
{
	public string payment_method_id { get; set; }

	public Guid? target_user_id { get; set; }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using TradePlatform.Api.DTOs.Refunds;

namespace TradePlatform.Api.Services.stripe;

public class StripeRefundService : IStripeRefundService
{
	private readonly StripeClient _stripeClient;

	public StripeRefundService(StripeClient stripeClient)
	{
		_stripeClient = stripeClient;
	}

	public async Task<Refund> CreateStripeRefundAsync(RefundViewDto refundview, Guid refund_id)
	{
		RefundCreateOptions refundCreateOptions = new RefundCreateOptions();
		refundCreateOptions.PaymentIntent = refundview.payment_intent_id;
		refundCreateOptions.Amount = (long)(refundview.refund_gross * 100m);
		RefundCreateOptions refundCreateOptions2 = refundCreateOptions;
		string stripe_refund_reason = refundview.stripe_refund_reason;
		string reason;
		if (stripe_refund_reason == "duplicate")
		{
			reason = "duplicate";
		}
		else
		{
			reason = ((!(stripe_refund_reason == "fraudulent")) ? "requested_by_customer" : "fraudulent");
		}
		refundCreateOptions2.Reason = reason;
		refundCreateOptions.Metadata = new Dictionary<string, string>
		{
			{
				"refund_id",
				refundview.id.ToString()
			},
			{
				"entity_id",
				refundview.entity_id.ToString()
			},
			{
				"entity_type_id",
				refundview.entity_type_id.ToString()
			},
			{ "refund_source", "MyTradesShop" }
		};
		RefundCreateOptions refundOptions = refundCreateOptions;
		RefundService refundService = new RefundService(_stripeClient);
		return await refundService.CreateAsync(refundOptions);
	}
}

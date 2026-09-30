using System;
using System.Threading.Tasks;
using Stripe;
using TradePlatform.Api.DTOs.Refunds;

namespace TradePlatform.Api.Services.stripe;

public interface IStripeRefundService
{
	Task<Refund> CreateStripeRefundAsync(RefundViewDto refundview, Guid refund_id);
}

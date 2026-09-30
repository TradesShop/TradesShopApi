using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IPaymentsRepository
{
	Task<string> CreateSetupIntentAsync(Guid callerId, UserType callerType, Guid? targetUserId);

	Task<object> AttachPaymentMethodAsync(Guid effectiveUserId, UserType callerType, string payment_method_id);

	Task<object> SubscribeAsync(Guid callerId, UserType callerType, string priceId, string paymentMethodId, Guid? targetUserId);

	Task CancelSubscriptionAsync(Guid callerId, UserType callerType, string subscriptionId, Guid? targetUserId);

	Task MarkSucceededAsync(string payment_intent_id);

	Task MarkFailedAsync(string payment_intent_id);

	Task<PaymentsM?> GetByStripePaymentIntentIdAsync(string stripe_payment_intent_id);

	Task MarkRefundedAsync(Guid payment_id, decimal amount, string stripe_refund_id);

	Task CreateAsync(PaymentsM model);

	Task<IEnumerable<PaymentsM>> GetByInvoiceIdAsync(Guid invoice_id);

	Task<PaymentsM?> GetByIdAsync(Guid id);

	Task<IEnumerable<PaymentsM>> GetByUserIdAsync(Guid user_id);

	Task InsertPaymentAsync(PaymentsM payment);
}

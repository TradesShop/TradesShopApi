using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Payments;

public interface IPaymentsService
{
	Task<PaymentsM?> GetPaymentAsync(Guid payment_id);

	Task<IEnumerable<PaymentsM>> GetPaymentsByInvoiceAsync(Guid invoice_id);

	Task<IEnumerable<PaymentsM>> GetPaymentsByUserAsync(Guid user_id);

	Task<PaymentsM> RecordPaymentAsync(Guid user_id, Guid invoice_id, string stripe_payment_intent_id, string? stripe_charge_id, decimal amount, string currency, string status);
}

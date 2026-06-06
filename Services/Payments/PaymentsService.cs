using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IPaymentsRepository _paymentsRepo;
        private readonly IInvoicesTshRepository _invoicesRepo;

        public PaymentsService(
            IPaymentsRepository paymentsRepo,
            IInvoicesTshRepository invoicesRepo)
        {
            _paymentsRepo = paymentsRepo;
            _invoicesRepo = invoicesRepo;
        }

        public async Task<PaymentsM?> GetPaymentAsync(Guid payment_id)
        {
            var payment = await _paymentsRepo.GetByIdAsync(payment_id);
            if (payment == null) return null;

            return payment;
        }

        public async Task<IEnumerable<PaymentsM>> GetPaymentsByInvoiceAsync(Guid invoice_id)
        {
            var payments = await _paymentsRepo.GetByInvoiceIdAsync(invoice_id);
            return payments;
        }

        public async Task<IEnumerable<TradePlatform.Api.Models.PaymentsM>> GetPaymentsByUserAsync(Guid user_id)
        {
            var invoices = await _invoicesRepo.GetByUserAsync(user_id);

            var allPayments = new List<PaymentsM>();

            foreach (var invoice in invoices)
            {
                var payments = await _paymentsRepo.GetByInvoiceIdAsync(invoice.id);
                allPayments.AddRange(payments);
            }

            return allPayments;
        }

        public async Task<PaymentsM> RecordPaymentAsync(
            Guid user_id,
            Guid invoice_id,
            string stripe_payment_intent_id,
            string? stripe_charge_id,
            decimal amount,
            string currency,
            string status)
        {
            var now = DateTime.UtcNow;

            var payment = new TradePlatform.Api.Models.PaymentsM
            {
                id = Guid.NewGuid(),
                user_id = user_id,
                invoice_id = invoice_id,
                stripe_payment_intent_id = stripe_payment_intent_id,
                stripe_charge_id = stripe_charge_id,
                amount = amount,
                currency = currency,
                status = status,
                created_at = now
            };

            await _paymentsRepo.CreateAsync(payment);

            return payment;
        }

       
    }
}

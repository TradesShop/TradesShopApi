using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Invoices;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Services.stripe
{
    public class StripeInvoiceWebhookService : IStripeInvoiceWebhookService
    {
        private readonly IInvoicesTshRepository _invoicesRepo;
        private readonly IInvoicesTshService _invoicesService;
        private readonly IPaymentsService _paymentsService;
        private readonly IPaymentsRepository _paymentsRepo;

        public StripeInvoiceWebhookService(
            IInvoicesTshRepository invoicesRepo,
            IInvoicesTshService invoicesService,
            IPaymentsService paymentsService,
            IPaymentsRepository paymentsRepo)
        {
            _invoicesRepo = invoicesRepo;
            _invoicesService = invoicesService;
            _paymentsService = paymentsService;
            _paymentsRepo = paymentsRepo;
        }

        public async Task HandleInvoicePaidAsync(
            string stripe_invoice_id,
            string stripe_payment_intent_id,
            string stripe_customer_id,
            decimal amount_paid,
            string currency)
        {
            var invoice = await _invoicesRepo.GetByStripeInvoiceIdAsync(stripe_invoice_id);
            if (invoice == null) return;

            await _paymentsService.RecordPaymentAsync(
                invoice.user_id,
                invoice.id,
                stripe_payment_intent_id,
                stripe_charge_id: null,
                amount: amount_paid,
                currency: currency,
                status: "succeeded"
            );

            await _invoicesService.MarkInvoicePaidAsync(invoice.id, DateTime.UtcNow);
        }

        public async Task HandleInvoicePaymentFailedAsync(
            string stripe_invoice_id,
            string stripe_payment_intent_id)
        {
            var invoice = await _invoicesRepo.GetByStripeInvoiceIdAsync(stripe_invoice_id);
            if (invoice == null) return;

            await _invoicesService.MarkInvoiceFailedAsync(invoice.id);
        }
        public async Task HandleRefundAsync(string payment_intent_id, decimal amount, string refund_id)
        {
            var payment = await _paymentsRepo.GetByStripePaymentIntentIdAsync(payment_intent_id);
            if (payment == null) return;

            await _paymentsRepo.MarkRefundedAsync(payment.id, amount, refund_id);

            var invoice = await _invoicesRepo.GetByIdAsync(payment.invoice_id);

            if (amount == payment.amount)
                invoice.status = "refunded";
            else
                invoice.status = "partially_refunded";

            invoice.updated_at = DateTime.UtcNow;
            await _invoicesRepo.UpdateAsync(invoice);
        }

    }
}

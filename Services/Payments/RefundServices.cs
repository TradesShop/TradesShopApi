using Stripe;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Payments;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments
{
    public class RefundServices : IRefundServices
    {
        private readonly IPaymentsRepository _paymentsRepo;
        private readonly IInvoicesTshRepository _invoicesRepo;
        private readonly IConfiguration _config;

        public RefundServices(
            IPaymentsRepository paymentsRepo,
            IInvoicesTshRepository invoicesRepo,
            IConfiguration config)
        {
            _paymentsRepo = paymentsRepo;
            _invoicesRepo = invoicesRepo;
            _config = config;
        }

        public async Task<bool> RefundPaymentAsync(RefundRequestDto dto)
        {
            // ---------------------------------------------
            // 0. Load payment
            // ---------------------------------------------
            var payment = await _paymentsRepo.GetByIdAsync(dto.PaymentId);
            if (payment == null)
                return false;

            // ---------------------------------------------
            // 1. Stripe Refund
            // ---------------------------------------------
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = payment.stripe_payment_intent_id,
                Amount = (long)(dto.Amount * 100),
                Reason = dto.Reason
            });

            // ---------------------------------------------
            // 2. Update internal payment record
            // ---------------------------------------------
            await _paymentsRepo.MarkRefundedAsync(
                payment.id,
                dto.Amount,
                refund.Id
            );
            
            // ---------------------------------------------
            // 3. Update invoice status
            // ---------------------------------------------
            var invoice = await _invoicesRepo.GetByIdAsync(payment.invoice_id);
            if (invoice == null)
                return true; // payment refunded but invoice missing (should not happen)

            if (dto.Amount == payment.amount)
            {
                invoice.status = "refunded";
            }
            else
            {
                invoice.status = "partially_refunded";
            }

            invoice.updated_at = DateTime.UtcNow;
            await _invoicesRepo.UpdateAsync(invoice);

            return true;
        }
    }

    // Stripe wrapper to avoid name conflict with your RefundService
    //public class RefundServiceStripe : RefundServices { }
}

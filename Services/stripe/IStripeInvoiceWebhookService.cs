namespace TradePlatform.Api.Services.stripe
{
    public interface IStripeInvoiceWebhookService
    {
        Task HandleInvoicePaidAsync(
            string stripe_invoice_id,
            string stripe_payment_intent_id,
            string stripe_customer_id,
            decimal amount_paid,
            string currency
        );

        Task HandleInvoicePaymentFailedAsync(
            string stripe_invoice_id,
            string stripe_payment_intent_id
        );
    }
}

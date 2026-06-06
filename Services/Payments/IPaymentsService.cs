using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.DTOs.Stripe;

namespace TradePlatform.Api.Services.Payments
{
    public interface IPaymentsService
    {
        Task<PaymentsM?> GetPaymentAsync(Guid payment_id);
        Task<IEnumerable<PaymentsM>> GetPaymentsByInvoiceAsync(Guid invoice_id);
        Task<IEnumerable<PaymentsM>> GetPaymentsByUserAsync(Guid user_id);
        Task<PaymentsM> RecordPaymentAsync(
              Guid user_id
            , Guid invoice_id
            , string stripe_payment_intent_id
            , string? stripe_charge_id
            , decimal amount
            , string currency
            , string status
            );

        //Task<IReadOnlyList<PaymentMethodDto>> GetPaymentMethodsAsync(Guid user_id);


        //Task<PaymentMethodDto> AddPaymentMethodAsync(
        //    Guid user_id,
        //    AddPaymentMethodRequestDto request,
        //    Guid? updated_by
        //);

        //Task SetPrimaryAsync(
        //    Guid user_id,
        //    Guid paymentmethod_id,
        //    Guid? updated_by
        //);

        //Task DeleteAsync(
        //    Guid user_id,
        //    Guid paymentmethod_id,
        //    Guid? updated_by
        //);
       
        
    }
}

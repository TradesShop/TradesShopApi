using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Payments
{
    public interface IPaymentMethdTdsService
    {
        Task<PaymentMethod_db> GetDefaultPaymentMethodAsync(Guid user_id);
        Task UpdatePaymentMethodAsync(
         string stripe_payment_method_id,
         string? name_on_card,
         int exp_month,
         int exp_year,
         Guid effectiveUserId);
    }
}

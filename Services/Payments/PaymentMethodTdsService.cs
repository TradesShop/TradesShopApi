using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments
{
    public class PaymentMethodTdsService:IPaymentMethdTdsService
    {
        private readonly IPaymentMethodRepository _payMethodRepo;
       
        public PaymentMethodTdsService(
            IPaymentMethodRepository payMethodRepo)
        {
            _payMethodRepo = payMethodRepo;
            
        }
        public async Task<PaymentMethod_db> GetDefaultPaymentMethodAsync(Guid user_id) {
            return await _payMethodRepo.GetDefaultPaymentMethodAsync(user_id);
         }
        public async Task UpdatePaymentMethodAsync(
         string stripe_payment_method_id,
         string? name_on_card,
         int exp_month,
         int exp_year,
         Guid effectiveUserId)
        {
             await UpdatePaymentMethodAsync(stripe_payment_method_id, name_on_card, exp_month, exp_year, effectiveUserId);
        }
    }
}

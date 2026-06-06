
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethod_db>> GetPaymentMethodsAsync(Guid userId);
        Task<PaymentMethod_db?> GetDefaultPaymentMethodAsync(Guid user_id);
        Task<Guid> AddPaymentMethodAsync(PaymentMethod_db model);
        Task SetDefaultPaymentMethodAsync(Guid userId, string stripe_paymentmethod_id);
        Task SoftDeletePaymentMethodAsync(Guid userId, string stripe_paymentmethod_id);
        Task UpdatePaymentMethodAsync(
                        string stripe_payment_method_id,
                        string? name_on_card,
                        int exp_month,
                        int exp_year,
                        Guid updated_by);
    }
}

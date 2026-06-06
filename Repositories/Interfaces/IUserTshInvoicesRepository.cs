using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IUserTshInvoicesRepository
    {
        Task<UserInvoices?> GetByStripeInvoiceIdAsync(string stripe_invoiceid);
        Task<IReadOnlyList<UserInvoices>> GetByUserAsync(Guid user_id);

        Task InsertAsync(UserInvoices entity);
        Task UpdateAsync(UserInvoices entity);
    }
}

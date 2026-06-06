using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IBundlesCreditRepository
    {
        Task<IEnumerable<CreditBundles>> GetAllBundlesAsync();
        Task<CreditBundles?> GetByIdAsync(Guid bundle_id);
        Task CreateAsync(CreditBundles model);
        Task UpdateAsync(CreditBundles model);
        Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync();

    }
}

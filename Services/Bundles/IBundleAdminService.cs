using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Bundles
{
    public interface IBundleAdminService
    {
        Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync();

        // READ
        Task<IEnumerable<CreditBundles>> GetAllBundlesAsync();
        Task<CreditBundles?> GetBundleAsync(Guid bundle_id);
        //Task<IEnumerable<BundlePrices>> GetBundlePricesAsync(Guid bundle_id);
        Task<BundlePrices?> GetPriceAsync(Guid price_id);

        // CREATE
        Task CreateBundleAsync(CreditBundles model);
        Task CreatePriceAsync(BundlePrices model);

        // UPDATE
        Task UpdateBundleAsync(CreditBundles model);
        Task UpdatePriceAsync(BundlePrices model);
    }
}

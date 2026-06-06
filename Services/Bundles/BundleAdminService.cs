using System.Reflection.Metadata;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Bundles
{
    public class BundleAdminService : IBundleAdminService
    {
        private readonly IBundlesCreditRepository _bundlesRepo;
        private readonly IBundlePricesRepository _pricesRepo;

        public BundleAdminService(
            IBundlesCreditRepository bundlesRepo,
            IBundlePricesRepository pricesRepo)
        {
            _bundlesRepo = bundlesRepo;
            _pricesRepo = pricesRepo;
        }

        // ------------------------------------------------------------
        // READ OPERATIONS
        // ------------------------------------------------------------

        public async Task<IEnumerable<CreditBundles>> GetAllBundlesAsync()
        {
            return await _bundlesRepo.GetAllBundlesAsync();
        }

        public async Task<CreditBundles?> GetBundleAsync(Guid bundle_id)
        {
            return await _bundlesRepo.GetByIdAsync(bundle_id);
        }

        //public async Task<IEnumerable<BundlePrices>> GetBundlePricesAsync(Guid bundle_id)
        //{
        //    return await _pricesRepo.GetPricesByBundleIdAsync(bundle_id);
        //}

        public async Task<BundlePrices?> GetPriceAsync(Guid price_id)
        {
            return await _pricesRepo.GetPricesByIdAsync(price_id);
        }

        // ------------------------------------------------------------
        // CREATE OPERATIONS
        // ------------------------------------------------------------

        public async Task CreateBundleAsync(CreditBundles model)
        {
            await _bundlesRepo.CreateAsync(model);
        }

        public async Task CreatePriceAsync(BundlePrices model)
        {
            await _pricesRepo.CreateAsync(model);
        }

        // ------------------------------------------------------------
        // UPDATE OPERATIONS
        // ------------------------------------------------------------

        public async Task UpdateBundleAsync(CreditBundles model)
        {
            await _bundlesRepo.UpdateAsync(model);
        }

        public async Task UpdatePriceAsync(BundlePrices model)
        {
            await _pricesRepo.UpdateAsync(model);
        }

        public async Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync()
        {
            return await _bundlesRepo.GetActiveBundlesAsync();
        }
    }
   
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.BundleCredit;

namespace TradePlatform.Api.Services.Bundles;

public interface IBundleAdminService
{
	Task<IEnumerable<CreditBundles>> GetActiveBundlesAsync();

	Task<IEnumerable<CreditBundles>> GetAllBundlesAsync();

	Task<CreditBundles?> GetBundleAsync(Guid bundle_id);

	Task<BundlePrices?> GetPriceAsync(Guid price_id);

	Task CreateBundleAsync(CreditBundles model);

	Task CreatePriceAsync(BundlePrices model);

	Task UpdateBundleAsync(CreditBundles model);

	Task UpdatePriceAsync(BundlePrices model);
}

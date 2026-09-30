using System;
using System.Threading.Tasks;
using TradePlatform.Api.Models.BundleCredit;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IBundlePricesRepository
{
	Task<BundlePrices> GetPricesByIdAsync(Guid bundle_id);

	Task CreateAsync(BundlePrices model);

	Task UpdateAsync(BundlePrices model);
}

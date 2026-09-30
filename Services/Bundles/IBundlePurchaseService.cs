using System;
using System.Threading;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models.BundleCredit;

namespace TradePlatform.Api.Services.Bundles;

public interface IBundlePurchaseService
{
	Task<BundlePurchaseResponse> CreateCheckoutSessionAsync(Guid userId, Guid bundleId, Guid bundlePriceId, CancellationToken cancellationToken);

	Task CreditBundlePurchaseCompletedAsync(BundlePurchaseCompletedDto dto);

	Task OnBundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto);
}

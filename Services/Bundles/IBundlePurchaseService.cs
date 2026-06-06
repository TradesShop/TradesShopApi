using TradePlatform.Api.DTOs.Bundles;

namespace TradePlatform.Api.Services.Bundles
{
    public interface IBundlePurchaseService
    {
        Task<string> CreateCheckoutSessionAsync(
           Guid user_id,
           Guid bundle_id,
           Guid bundle_price_id,
           string successUrl,
           string cancelUrl);
        Task OnBundleCheckoutCompletedAsync(BundleCheckoutCompletedDto dto);
        Task OnBundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto);
        
    }
    
}


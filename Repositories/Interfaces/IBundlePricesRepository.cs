using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IBundlePricesRepository
    {
        Task<BundlePrices> GetPricesByIdAsync(Guid bundle_id);
       
        Task CreateAsync(BundlePrices model);
        Task UpdateAsync(BundlePrices model);
       
       
    }
}

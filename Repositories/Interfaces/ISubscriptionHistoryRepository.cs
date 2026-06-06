using TradePlatform.Api.Models;
namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ISubscriptionHistoryRepository
    {
        Task<long> SubscriptionHistoryInsertAsync(SubscriptionHistory history);
    }
}

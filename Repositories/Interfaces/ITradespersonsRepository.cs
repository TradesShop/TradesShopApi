using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ITradespersonsRepository
    {
        Task CreateAsync(Tradesperson tradesperson);
    }
}

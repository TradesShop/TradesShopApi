
using TradePlatform.Api.Models;
namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IEntityTypesRepository
    {
        Task<IReadOnlyList<EntityType>> GetAllAsync();
        Task<EntityType?> GetByNameAsync(string name);
    }
}

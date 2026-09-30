using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IEntityTypesRepository
{
	Task<IReadOnlyList<EntityTypes>> GetAllAsync();

	Task<EntityTypes?> GetByNameAsync(string name);
}


using Dapper;
using TradePlatform.Api.Models;
using TradePlatform.Api.Data;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class EntityTypesRepository : IEntityTypesRepository
    {
        private readonly DapperContext _context;

        public EntityTypesRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<EntityType>> GetAllAsync()
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<EntityType>(
                "SELECT id, name, description FROM entity_types");
            return result.ToList();
        }

        public async Task<EntityType?> GetByNameAsync(string name)
        {
            using var conn = _context.CreateOpenConnection();
            return await conn.QueryFirstOrDefaultAsync<EntityType>(
                "SELECT id, name, description FROM entity_types WHERE name = @name",
                new { name });
        }
    }
}

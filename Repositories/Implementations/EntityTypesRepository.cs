using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class EntityTypesRepository : IEntityTypesRepository
{
	private readonly DapperContext _context;

	public EntityTypesRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<EntityTypes>> GetAllAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		return (await conn.QueryAsync<EntityTypes>("SELECT id, name, description FROM entity_types")).ToList();
	}

	public async Task<EntityTypes?> GetByNameAsync(string name)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		return await conn.QueryFirstOrDefaultAsync<EntityTypes>("SELECT id, name, description FROM entity_types WHERE name = @name", new { name });
	}
}

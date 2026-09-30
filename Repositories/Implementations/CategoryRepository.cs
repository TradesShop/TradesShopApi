using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class CategoryRepository : ICategoryRepository
{
	private readonly DapperContext _context;

	public CategoryRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<categories>> GetCategoriesAsync(string? category_ids)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { category_ids };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<categories>("[dbo].[usp_categories_get_by_ids]", param, null, null, commandType);
	}

	public async Task<categories> CategoryUpsertAsync(categories ctgry)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { ctgry.id, ctgry.name, ctgry.slug, ctgry.description, ctgry.icon, ctgry.is_active, ctgry.parent_id, ctgry.sortorder, ctgry.is_default, ctgry.category_type_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<categories>("[dbo].[usp_category_upsert]", param, null, null, commandType);
	}

	public async Task<IEnumerable<category>> GetCategoriesForPublic()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<category>("[dbo].[usp_categories_for_public]", null, null, null, commandType);
	}

	public async Task<IEnumerable<category>> GetCategoriesListAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<category>("[dbo].[usp_categories_list]", null, null, null, commandType);
	}

	public async Task<IEnumerable<category>> GetCategoriesListDefaultAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<category>("[dbo].[usp_categories_list_default]", null, null, null, commandType);
	}

	public async Task<IEnumerable<category>> CategoriesSearchAsync(string search_term)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { search_term };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<category>("[dbo].[usp_categories_search]", param, null, null, commandType);
	}

	public async Task<List<CategorySkillFlatDto>> GetCategoriesWithSkillsAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return (await conn.QueryAsync<CategorySkillFlatDto>("usp_categories_with_skills_get_all", null, null, null, commandType))?.ToList() ?? new List<CategorySkillFlatDto>();
	}

	public async Task<IEnumerable<CategoryTypes>> GetCategorieTypesAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<CategoryTypes>("[dbo].[usp_category_types_list]", null, null, null, commandType);
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface ICategoryRepository
{
	Task<IEnumerable<category>> GetCategoriesForPublic();

	Task<IEnumerable<categories>> GetCategoriesAsync(string? category_ids);

	Task<categories> CategoryUpsertAsync(categories ctgry);

	Task<List<CategorySkillFlatDto>> GetCategoriesWithSkillsAsync();

	Task<IEnumerable<category>> GetCategoriesListAsync();

	Task<IEnumerable<category>> GetCategoriesListDefaultAsync();

	Task<IEnumerable<category>> CategoriesSearchAsync(string search_term);

	Task<IEnumerable<CategoryTypes>> GetCategorieTypesAsync();
}

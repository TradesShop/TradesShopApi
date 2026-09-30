using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Categories;

public class CategoryService : ICategoryService
{
	private readonly ICategoryRepository _ctgryRepo;

	public CategoryService(ICategoryRepository ctgryRepo)
	{
		_ctgryRepo = ctgryRepo;
	}

	public async Task<IEnumerable<category>> GetCategoriesForPublic()
	{
		return await _ctgryRepo.GetCategoriesForPublic();
	}

	public async Task<IEnumerable<categories>> GetCategoriesAsync(string? category_ids)
	{
		return await _ctgryRepo.GetCategoriesAsync(category_ids);
	}

	public async Task<categories> CategoryUpsertAsync(categories ctgry)
	{
		return await _ctgryRepo.CategoryUpsertAsync(ctgry);
	}

	public async Task<IEnumerable<category>> GetCategoriesListAsync()
	{
		return await _ctgryRepo.GetCategoriesListAsync();
	}

	public async Task<IEnumerable<category>> GetCategoriesListDefaultAsync()
	{
		return await _ctgryRepo.GetCategoriesListDefaultAsync();
	}

	public async Task<IEnumerable<CategoryTypes>> GetCategorieTypesAsync()
	{
		return await _ctgryRepo.GetCategorieTypesAsync();
	}

	public async Task<IEnumerable<category>> CategoriesSearchAsync(string search_term)
	{
		return await _ctgryRepo.CategoriesSearchAsync(search_term);
	}

	public async Task<List<CategoryResponseDto>> GetCategoriesWithSkillsAsync()
	{
		List<CategorySkillFlatDto> anydata = (await _ctgryRepo.GetCategoriesWithSkillsAsync())?.Where((CategorySkillFlatDto x) => x != null).ToList() ?? new List<CategorySkillFlatDto>();
		return (from x in anydata
			group x by new { x.category_id, x.category_name } into g
			select new CategoryResponseDto
			{
				id = g.Key.category_id,
				name = (g.Key.category_name ?? string.Empty),
				children = (from x in g
					where x.skill_id.HasValue
					select new CategoryChildDto
					{
						id = x.skill_id.Value,
						name = (x.skill_name ?? string.Empty)
					} into x
					orderby x.name
					select x).ToList()
			} into x
			orderby x.name
			select x).ToList();
	}
}

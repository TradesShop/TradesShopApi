using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.Models.Categories;
using TradePlatform.Api.Models.Locations;
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
    public async Task<category_meta_seo> GetCategorySeoBySlugAsync(string ctgry_slug)
    {
        return await _ctgryRepo.GetCategorySeoBySlugAsync(ctgry_slug);
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
    public async Task<category_meta_seo> GetCategoryServiceWithSeoForPublic(string service_slug) {
        return await _ctgryRepo.GetCategoryServiceWithSeoForPublic(service_slug);
    }    
    public async Task<IEnumerable<category_services>> GetCategoriesWithServicesForPublic()
    {
        return await _ctgryRepo.GetCategoriesWithServicesForPublic();
    }

    public async Task<IEnumerable<category_meta_seo>> GetCategoriesWithSeoForPublic()
    {
        return await _ctgryRepo.GetCategoriesWithSeoForPublic();
    }
    public async Task<category_seo> GetCategorySeoViewAsync(int category_id)
    {
        return await _ctgryRepo.GetCategorySeoViewAsync(category_id);
    }
    public async Task<category_seo> CategorySeoUpsertAsync(category_seo ctgry_seo)
    {
        return await _ctgryRepo.CategorySeoUpsertAsync(ctgry_seo);
    }

    public async Task<IEnumerable<service_sitemap>> GetAllServicesForSiteMapAsync()
    {
        return await _ctgryRepo.GetAllServicesForSiteMapAsync();
    }


    /*Service Locations here*/
    public async Task<int> GetServiceLocationsSiteMapCountAsync()
    {
        return await _ctgryRepo.GetServiceLocationsSiteMapCountAsync();
    }
    public async Task<IEnumerable<locations_sitemap>> GetServiceLocationsSiteMapAsync(int offset, int limit)
	{
		return await _ctgryRepo.GetServiceLocationsSiteMapAsync(offset, limit );
	}
	public async Task<Dictionary<string, List<LocationSummary>>> GetLocationsGroupedByRegionAsync(string? country = null)
    {        
        return await _ctgryRepo.GetLocationsGroupedByRegionAsync(country);
    }
    public async Task<location_seo> GetLocationSeoBySlugAsync(string loc_slug)
    {
        return await _ctgryRepo.GetLocationSeoBySlugAsync(loc_slug);
    }
}

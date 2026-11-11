using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.Models.Categories;
using TradePlatform.Api.Models.Categories;
using TradePlatform.Api.Models.Locations;
using TradePlatform.Api.Services.Categories;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface ICategoryRepository
{

	Task<IEnumerable<categories>> GetCategoriesAsync(string? category_ids);

	Task<categories> CategoryUpsertAsync(categories ctgry);

	Task<List<CategorySkillFlatDto>> GetCategoriesWithSkillsAsync();

	Task<IEnumerable<category>> GetCategoriesListAsync();

	Task<IEnumerable<category>> GetCategoriesListDefaultAsync();

	Task<IEnumerable<category>> CategoriesSearchAsync(string search_term);

	Task<IEnumerable<CategoryTypes>> GetCategorieTypesAsync();
	Task<category_seo> GetCategorySeoViewAsync(int category_id);
	Task<category_seo> CategorySeoUpsertAsync(category_seo ctgry_seo);
	Task<IEnumerable<category>> GetCategoriesForPublic();
	Task<category_meta_seo> GetCategoryServiceWithSeoForPublic(string service_slug);
    Task<IEnumerable<category_services>> GetCategoriesWithServicesForPublic();
    Task<IEnumerable<category_meta_seo>> GetCategoriesWithSeoForPublic();
    Task<category_meta_seo> GetCategorySeoBySlugAsync(string ctgry_slug);

	/*Service Locations*/
	Task<Dictionary<string, List<LocationSummary>>> GetLocationsGroupedByRegionAsync(string? country = null);
	Task<location_seo> GetLocationSeoBySlugAsync(string loc_slug);

}

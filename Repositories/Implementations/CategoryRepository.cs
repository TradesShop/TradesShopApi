using Amazon.S3.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Stripe.Tax;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.DTOs.CategoryTypes;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Models.Categories;
using TradePlatform.Api.Models.Locations;
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
		var param = new { 
			  ctgry.id
			, ctgry.name
			, ctgry.slug
			, ctgry.description
			, ctgry.icon
			, ctgry.is_active
			, ctgry.parent_id
			, ctgry.sortorder
			, ctgry.is_default
			, ctgry.category_type_id 
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<categories>("[dbo].[usp_category_upsert]", param, null, null, commandType);
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

    public async Task<category_seo> GetCategorySeoViewAsync(int category_id)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        var param = new
        {
            category_id = category_id
        };
        var anyctgry_seo= await conn.QueryFirstOrDefaultAsync<category_seo>(
            "[dbo].[usp_category_seo_get_by_category_id]",
            param,
            commandType: CommandType.StoredProcedure
        );
		return anyctgry_seo;
    }
    public async Task<category_seo> CategorySeoUpsertAsync(category_seo ctgry_seo)
    {
        using IDbConnection conn = _context.CreateOpenConnection();

        var param = new
        {
            category_id = ctgry_seo.category_id,
            category_overview = ctgry_seo.category_overview,
            alias_slugs = ctgry_seo.alias_slugs,
            seo_title_template = ctgry_seo.seo_title_template,
            seo_description_template = ctgry_seo.seo_description_template,
            canonical_url_template = ctgry_seo.canonical_url_template,
            meta_robots = ctgry_seo.meta_robots ?? "index, follow",
            h1_template = ctgry_seo.h1_template,
            intro_content = ctgry_seo.intro_content,
            local_content_template = ctgry_seo.local_content_template,
            services_heading = ctgry_seo.services_heading,
            services_json = ctgry_seo.services_json,
            related_keywords = ctgry_seo.related_keywords,
            why_choose_heading = ctgry_seo.why_choose_heading,
            why_choose_content = ctgry_seo.why_choose_content,
            faq_template_json = ctgry_seo.faq_template_json,
            og_title_template = ctgry_seo.og_title_template,
            og_description_template = ctgry_seo.og_description_template,
            og_image_url = ctgry_seo.og_image_url,
            is_active = ctgry_seo.is_active
        };

        var anyctgry_seo= await conn.QueryFirstOrDefaultAsync<category_seo>(
            "[dbo].[usp_category_seo_upsert]", // Note: ensure stored procedure name is correct
            param,
            commandType: CommandType.StoredProcedure
        );
		return anyctgry_seo;
    }
    public async Task<IEnumerable<category>> GetCategoriesForPublic()
    {
        using IDbConnection conn = _context.CreateOpenConnection();       
        return await conn.QueryAsync<category>(
            "[dbo].[usp_categories_for_public]"
            , null
            , commandType: CommandType.StoredProcedure);
    }
    public async Task<IEnumerable<category_services>> GetCategoriesWithServicesForPublic()
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        return await conn.QueryAsync<category_services>(
            "[dbo].[usp_categories_with_services_get_all]"
            , null
            , commandType: CommandType.StoredProcedure);
    }
    public async Task<IEnumerable<category_meta_seo>> GetCategoriesWithSeoForPublic()
    {
        using IDbConnection conn = _context.CreateOpenConnection();     
        return await conn.QueryAsync<category_meta_seo>(
            "[dbo].[usp_category_seo_get_all_public]"
            , null
			,  commandType: CommandType.StoredProcedure);
    }
    /*this one using ion live*/
    public async Task<category_meta_seo> GetCategoryServiceWithSeoForPublic(string service_slug)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        var param = new { slug = service_slug };
        var anyctgry_seo = await conn.QueryFirstOrDefaultAsync<category_meta_seo>(
            "[dbo].[usp_category_seo_route_resolve]"
            , param
            , commandType: CommandType.StoredProcedure
            );
        return anyctgry_seo;
    }
    public async Task<category_meta_seo> GetCategorySeoBySlugAsync(string ctgry_slug)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        var param = new { slug = ctgry_slug };
        var anyctgry_seo = await conn.QueryFirstOrDefaultAsync<category_meta_seo>(
			"[dbo].[usp_category_seo_get_by_slug]"
            ,param
            , commandType:CommandType.StoredProcedure
			);
        return anyctgry_seo;
    }

    public async Task<IEnumerable<service_sitemap>> GetAllServicesForSiteMapAsync()
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        return await conn.QueryAsync<service_sitemap>(
            "[dbo].[usp_category_services_for_sitemap]"
            , null
            , commandType: CommandType.StoredProcedure);
    }


    /*Service Locations*/
    public async Task<int> GetServiceLocationsSiteMapCountAsync()
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        const string sql = @"select count(1) as total from locations where is_active = 1";
        return await conn.ExecuteScalarAsync<int>(sql);        
    }
    public async Task<IEnumerable<locations_sitemap>> GetServiceLocationsSiteMapAsync(int offset, int limit)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        return await conn.QueryAsync<locations_sitemap>(
       "[dbo].[usp_locations_get_all_for_sitemap]",
       new
       {
           offset = offset,
           limit = limit
       },
       commandType: CommandType.StoredProcedure);
    }

    public async Task<Dictionary<string, List<LocationSummary>>> GetLocationsGroupedByRegionAsync(string? country = null)
    {
            using IDbConnection conn = _context.CreateOpenConnection();
            var rawLocations = await conn.QueryAsync<dynamic>(
            "[dbo].[usp_locations_get_grouped_by_region]",
            new { country = country },
            commandType: CommandType.StoredProcedure
        );

        var groupedResult = new Dictionary<string, List<LocationSummary>>();

            foreach (var row in rawLocations)
            {
                string region = row.region ?? "General";

                if (!groupedResult.ContainsKey(region))
                {
                    groupedResult[region] = new List<LocationSummary>();
                }
                string rawDbPostcodes = row.postcode_prefixes_json;
                // Deserialize the JSON string array generated by SQL
                //List<string> postcodes = !string.IsNullOrEmpty(rawDbPostcodes) ? JsonSerializer.Deserialize<List<string>>(rawDbPostcodes) ?? new() : new();

                groupedResult[region].Add(new LocationSummary
                {
                    id = row.id,
                    name = row.name,
                    slug = row.slug,
                    region = region,
                    county = row.county,
                    postcodes = !string.IsNullOrEmpty(rawDbPostcodes) ? JsonSerializer.Deserialize<List<string>>(rawDbPostcodes) ?? new() : new()
                });
            }

        return groupedResult;
        }
    public async Task<location_seo> GetLocationSeoBySlugAsync(string loc_slug)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        var param = new { slug = loc_slug };
        var anyloc_seo = await conn.QueryFirstOrDefaultAsync<location_seo>(
            "[dbo].[usp_locations_get_by_slug_public]"
            , param
            , commandType: CommandType.StoredProcedure
            );
        return anyloc_seo;
    }
}
	
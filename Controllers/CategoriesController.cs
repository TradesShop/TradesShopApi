using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Categories;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : BaseController
{
	private readonly ICategoryService _ctgryService;

	public CategoriesController(ICategoryService ctgryService)
	{
		_ctgryService = ctgryService;
	}

	[HttpGet("list")]
	public async Task<IActionResult> GetCategoriesListAsync()
	{
		return ApiOk(await _ctgryService.GetCategoriesListAsync());
	}

	[HttpGet("default")]
	public async Task<IActionResult> GetCategoriesDefaultAsync()
	{
		return ApiOk(await _ctgryService.GetCategoriesListDefaultAsync());
	}

	[HttpGet("search")]
	public async Task<IActionResult> CategoriesSearchAsync([FromQuery(Name = "q")] string search_term)
	{
		if (string.IsNullOrWhiteSpace(search_term))
		{
			return ApiOk(Enumerable.Empty<category>());
		}
		return ApiOk(await _ctgryService.CategoriesSearchAsync(search_term));
	}

	[HttpGet]
	public async Task<IActionResult> GetCategories([FromQuery(Name = "category_ids")] string? category_ids)
	{
		return ApiOk(await _ctgryService.GetCategoriesAsync(category_ids));
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetByCategory(string id)
	{
		return ApiOk(await _ctgryService.GetCategoriesAsync(id));
	}

	[HttpGet("withskills")]
	public async Task<IActionResult> GetCategoriesWithSkills()
	{
		return ApiOk(await _ctgryService.GetCategoriesWithSkillsAsync());
	}

	[HttpGet("types")]
	public async Task<IActionResult> GetCategorieTypesAsync()
	{
		return ApiOk(await _ctgryService.GetCategorieTypesAsync());
	}
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Services.Categories;
using static System.Net.WebRequestMethods;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _ctgryService;

        public CategoriesController(ICategoryService ctgryService,
            IHttpContextAccessor http
        ) : base(http)
        {
            _ctgryService = ctgryService;
        }

        // GET /api/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _ctgryService.GetCategoriesAsync(null);
            return Ok(categories);
        }

        // GET /api/categories/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByCategory(int id)
        {
            var categories = await _ctgryService.GetCategoriesAsync(id);
            return Ok(categories);
        }
        [HttpGet("withskills")]       
        public async Task<IActionResult> GetCategoriesWithSkills()
        {
            var result = await _ctgryService.GetCategoriesWithSkillsAsync();

            return ApiOk(result);
        }
    }
}

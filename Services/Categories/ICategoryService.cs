using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<category>> GetCategoriesAsync(int? id);
        Task<List<CategoryResponseDto>> GetCategoriesWithSkillsAsync();
    }
}

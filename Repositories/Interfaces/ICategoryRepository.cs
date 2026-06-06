using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<category>> GetCategoriesAsync(int? id);
        Task<List<CategorySkillFlatDto>> GetCategoriesWithSkillsAsync();
    }
}

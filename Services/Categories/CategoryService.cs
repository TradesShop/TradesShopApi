using Microsoft.EntityFrameworkCore;
using System.Data;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Categories
{
    public class CategoryService:ICategoryService
    {
        private readonly ICategoryRepository _ctgryRepo;

        public CategoryService(ICategoryRepository ctgryRepo)
        {
            _ctgryRepo = ctgryRepo;
        }
        public async Task<IEnumerable<category>> GetCategoriesAsync(int? id)
        {
            return await _ctgryRepo.GetCategoriesAsync(id);

           
        }
        public async Task<List<CategoryResponseDto>> GetCategoriesWithSkillsAsync()
        {
            var anydata = await _ctgryRepo.GetCategoriesWithSkillsAsync();

            // Safety: ensure no null items
            anydata = anydata?.Where(x => x != null).ToList()
                       ?? new List<CategorySkillFlatDto>();

            var result = anydata
                .GroupBy(x => new
                {
                    x.category_id,
                    x.category_name
                })
                .Select(g => new CategoryResponseDto
                {
                    id = g.Key.category_id,
                    name = g.Key.category_name ?? string.Empty,   // FIX

                    children = g
                        .Where(x => x.skill_id.HasValue)
                        .Select(x => new CategoryChildDto
                        {
                            id = x.skill_id!.Value,
                            name = x.skill_name ?? string.Empty   // FIX
                        })
                        .OrderBy(x => x.name)
                        .ToList()
                })
                .OrderBy(x => x.name)
                .ToList();

            return result;
        }

    }
}

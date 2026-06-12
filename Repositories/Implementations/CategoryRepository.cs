using Dapper;
using System.Data;
using System.Data.Common;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Categories;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;


namespace TradePlatform.Api.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DapperContext _context;


        public CategoryRepository(DapperContext context)
        {
            _context = context;

        }

        public async Task<IEnumerable<category>> GetCategoriesAsync(int? id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<category>(
                "usp_JobCategories_Get",
                new { id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<List<CategorySkillFlatDto>> GetCategoriesWithSkillsAsync()
        {
            using var conn = _context.CreateOpenConnection();

            var anyresult = await conn.QueryAsync<CategorySkillFlatDto>(
                "usp_categories_with_skills_get_all",
                commandType: CommandType.StoredProcedure
            );

            return anyresult?.ToList() ?? new List<CategorySkillFlatDto>();
        }
    }
}

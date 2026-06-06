using Dapper;
using System.Data;
using System.Data.SqlClient;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;


namespace TradePlatform.Api.Repositories
{
    public class subcategoryRepository
    {
        private readonly DapperContext _context;

        public subcategoryRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<subcategory>> GetsubCategoriesAsync(int? job_category_id)
        {
            using var conn = _context.CreateConnection();

            return await conn.QueryAsync<subcategory>(
                "usp_JobSubCategories_Get",
                new { job_category_id = job_category_id },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}

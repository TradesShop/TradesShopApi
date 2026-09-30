using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories;

public class subcategoryRepository
{
	private readonly DapperContext _context;

	public subcategoryRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<subcategory>> GetsubCategoriesAsync(int? job_category_id)
	{
		using IDbConnection conn = _context.CreateConnection();
		var param = new { job_category_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<subcategory>("usp_JobSubCategories_Get", param, null, null, commandType);
	}
}

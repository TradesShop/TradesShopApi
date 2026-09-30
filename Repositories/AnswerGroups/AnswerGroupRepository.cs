using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models.AnswerGroups;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.AnswerGroups;

public class AnswerGroupRepository : IAnswerGroupRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public AnswerGroupRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}

	public async Task<IEnumerable<AnswerGroupDto>> GetAllAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<AnswerGroupDto>("[dbo].[usp_answer_groups_list]", null, null, null, commandType);
	}

	public async Task<AnswerGroupDto> CreateAsync(AnswerGroupCreateModel model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.name, model.category_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QuerySingleAsync<AnswerGroupDto>("[dbo].[usp_answer_groups_create]", param, null, null, commandType);
	}

	public async Task<AnswerGroupDto> UpdateAsync(AnswerGroupUpdateModel model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.name, model.category_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QuerySingleAsync<AnswerGroupDto>("[dbo].[usp_answer_groups_update]", param, null, null, commandType);
	}
}

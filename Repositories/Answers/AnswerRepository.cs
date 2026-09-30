using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models.Answers;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Answers;

public class AnswerRepository : IAnswerRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public AnswerRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}

	public async Task<IEnumerable<AnswersDto>> GetByGroupAsync(int answer_group_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { answer_group_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<AnswersDto>("[dbo].[usp_answers_list_by_group_id]", param, null, null, commandType);
	}

	public async Task<AnswersDto> CreateAsync(AnswerCreateModel model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.answer_group_id, model.title, model.description, model.isactive, model.hascredit, model.credit, model.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<AnswersDto>("[dbo].[usp_answers_create]", param, null, null, commandType);
	}

	public async Task<AnswersDto> UpdateAsync(AnswerUpdateModel model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.title, model.description, model.isactive, model.hascredit, model.credit, model.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QuerySingleAsync<AnswersDto>("[dbo].[usp_answers_update]", param, null, null, commandType);
	}

	public async Task DeleteAnswerAsync(int id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_answer_delete]", param, null, null, commandType);
	}
}

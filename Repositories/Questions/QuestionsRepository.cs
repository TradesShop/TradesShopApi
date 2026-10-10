using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Questions;

public class QuestionsRepository : IQuestionsRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public QuestionsRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}



	public async Task<IEnumerable<QuestionsDto>> GetAllAsync()
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<QuestionsDto>("[dbo].[usp_questions_list]", null, null, null, commandType);
	}

	public async Task<QuestionsDto> GetByIdAsync(int id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<QuestionsDto>("[dbo].[usp_questions_get_by_id]", param, null, null, commandType);
	}

	public async Task<QuestionsDto> CreateAsync(QuestionCreateDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.title, model.description, model.answertype, model.isactive, model.group_id, model.answer_group_id, model.category_id, model.is_first_question, model.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<QuestionsDto>("[dbo].[usp_questions_create]", param, null, null, commandType);
	}

	public async Task<QuestionsDto> UpdateAsync(QuestionUpdateDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.title, model.description, model.answertype, model.isactive, model.group_id, model.answer_group_id, model.category_id, model.is_first_question, model.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<QuestionsDto>("[dbo].[usp_questions_update]", param, null, null, commandType);
	}

	public async Task<IEnumerable<QuestionFlowDto>> GetFlowsAsync(int question_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { question_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<QuestionFlowDto>("[dbo].[usp_question_flows_list]", param, null, null, commandType);
	}

	public async Task<QuestionFlowDto> CreateFlowAsync(QuestionFlowCreateDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.answer_id, model.next_question_id, model.condition_json, model.question_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<QuestionFlowDto>("[dbo].[usp_question_flows_create]", param, null, null, commandType);
	}

	public async Task<QuestionFlowDto> UpdateFlowAsync(QuestionFlowUpdateDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { model.id, model.answer_id, model.next_question_id, model.condition_json, model.question_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<QuestionFlowDto>("[dbo].[usp_question_flows_update]", param, null, null, commandType);
	}

	public async Task DeleteFlowAsync(int id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_question_flows_delete]", param, null, null, commandType);
	}
}

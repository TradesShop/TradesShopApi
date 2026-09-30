using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Questions;

namespace TradePlatform.Api.Repositories;

public class QuestionRepository
{
	private readonly DapperContext _context;

	public QuestionRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<QuestionDto?> GetQuestionsByCategory(int category_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		Dictionary<int, QuestionDto> questionDict = new Dictionary<int, QuestionDto>();
		Func<QuestionDto, AnswerQeDto, QuestionDto> map = (QuestionDto q, AnswerQeDto a) =>
		{
			if (!questionDict.TryGetValue(q.id, out QuestionDto value))
			{
				value = q;
				value.answers = new List<AnswerQeDto>();
				questionDict.Add(value.id, value);
			}
			if (a != null && a.answerid != 0)
			{
				value.answers.Add(a);
			}
			return value;
		};
		var param = new { category_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("usp_QuestionsByCategoryGetAsync", map, param, null, buffered: true, "answerid", null, commandType);
		return questionDict.Values.FirstOrDefault();
	}

	public async Task<int?> GetNextQuestionId(RequestForNextQue nQue)
	{
		DataTable table = new DataTable();
		table.Columns.Add("id", typeof(int));
		foreach (int id in nQue.answer_ids)
		{
			table.Rows.Add(id);
		}
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			question_id = nQue.question_id,
			answer_ids = table.AsTableValuedParameter("dbo.AnswerIdsList")
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<int?>("usp_QuestionNextIdGetAsync", param, null, null, commandType);
	}

	public async Task<QuestionDto?> GetQuestionWithAnswers(int question_id)
	{
		using IDbConnection connection = _context.CreateConnection();
		Dictionary<int, QuestionDto> questionDict = new Dictionary<int, QuestionDto>();
		Func<QuestionDto, AnswerQeDto, QuestionDto> map = (QuestionDto q, AnswerQeDto a) =>
		{
			if (!questionDict.TryGetValue(q.id, out QuestionDto value))
			{
				value = q;
				value.answers = new List<AnswerQeDto>();
				questionDict.Add(value.id, value);
			}
			if (a != null && a.answerid != 0)
			{
				value.answers.Add(a);
			}
			return value;
		};
		var param = new { question_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("usp_QuestionWithAnswersGetAsync", map, param, null, buffered: true, "answerid", null, commandType);
		return questionDict.Values.FirstOrDefault();
	}

	public async Task<List<QuestionDto>> GetQuestionsForPostJob(Guid job_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		Dictionary<int, QuestionDto> questionDict = new Dictionary<int, QuestionDto>();
		Func<QuestionDto, AnswerQeDto, QuestionDto> map = (QuestionDto q, AnswerQeDto a) =>
		{
			if (!questionDict.TryGetValue(q.id, out QuestionDto value))
			{
				value = q;
				value.answers = new List<AnswerQeDto>();
				questionDict.Add(value.id, value);
			}
			if (a != null && a.answerid > 0)
			{
				value.answers.Add(a);
			}
			return value;
		};
		var param = new { job_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("usp_job_get_questions_for_postjob", map, param, null, buffered: true, "answerid", null, commandType);
		return questionDict.Values.ToList();
	}

	public async Task UpsertAnswerAsync(AnswerUpsertDto auDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { auDto.job_id, auDto.question_id, auDto.answer_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.ExecuteAsync("usp_job_post_answer_upsert", param, null, null, commandType);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Repositories;

namespace TradePlatform.Api.Services.Questions;

public class QuestionsService : IQuestionsService
{
	private readonly QuestionRepository _repoQue;

	public QuestionsService(QuestionRepository repoQue)
	{
		_repoQue = repoQue;
	}

	public async Task<object> GetNextStep(RequestForNextQue nQue)
	{
		if (nQue.answer_ids == null || !nQue.answer_ids.Any())
		{
			return null;
		}
		int? nextQuestionId = await _repoQue.GetNextQuestionId(nQue);
		if (!nextQuestionId.HasValue)
		{
			return null;
		}
		return await _repoQue.GetQuestionWithAnswers(nextQuestionId.Value);
	}

	public async Task<List<QuestionDto>> GetQuestionsForPostJob(Guid job_id)
	{
		return await _repoQue.GetQuestionsForPostJob(job_id);
	}

	public async Task UpsertAnswerAsync(AnswerUpsertDto auDto)
	{
		await _repoQue.UpsertAnswerAsync(auDto);
	}
}

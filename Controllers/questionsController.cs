using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Services.Questions;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class questionsController : BaseController
{
	private readonly QuestionRepository _repoQue;

	private readonly IQuestionsService _queService;

	public questionsController(QuestionRepository repoQue, IQuestionsService queService)
	{
		_repoQue = repoQue;
		_queService = queService;
	}

	[HttpGet("by-category/{categoryId}")]
	public async Task<IActionResult> GetByCategory(int categoryId)
	{
		return ApiOk(await _repoQue.GetQuestionsByCategory(categoryId));
	}

	[HttpPost("nextquestion")]
	public async Task<object> GetNextStep([FromBody] RequestForNextQue nQue)
	{
		return ApiOk(await _queService.GetNextStep(nQue));
	}

	[HttpGet("postjob/{job_id}")]
	public async Task<IActionResult> GetQuestionsForPostJob(Guid job_id)
	{
		List<QuestionDto> questions = await _queService.GetQuestionsForPostJob(job_id);
		if (questions == null || questions.Count == 0)
		{
			return Ok(new
			{
				data = (object)null
			});
		}
		return ApiOk(questions);
	}

	[HttpPost("upsert")]
	public async Task<IActionResult> UpsertAnswerAsync([FromBody] AnswerUpsertDto auDto)
	{
		await _queService.UpsertAnswerAsync(auDto);
		return ApiOk(new
		{
			success = true
		});
	}
}

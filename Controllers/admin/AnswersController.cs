using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models.AnswerGroups;
using TradePlatform.Api.Models.Answers;
using TradePlatform.Api.Repositories.AnswerGroups;
using TradePlatform.Api.Repositories.Answers;

namespace TradePlatform.Api.Controllers.admin;

[Route("api/admin/[controller]")]
[ApiController]
public class AnswersController : BaseController
{
	private readonly IAnswerGroupRepository _groupRepo;

	private readonly IAnswerRepository _answerRepo;

	public AnswersController(IAnswerGroupRepository groupRepo, IAnswerRepository answerRepo)
	{
		_groupRepo = groupRepo;
		_answerRepo = answerRepo;
	}

	[HttpGet("answer-groups/list")]
	public async Task<IActionResult> ListGroups()
	{
		return ApiOk(await _groupRepo.GetAllAsync());
	}

	[HttpPost("answer-groups/create")]
	public async Task<IActionResult> CreateGroup([FromBody] AnswerGroupCreateModel model)
	{
		return ApiOk(new
		{
			id = await _groupRepo.CreateAsync(model)
		});
	}

	[HttpPost("answer-groups/update")]
	public async Task<IActionResult> UpdateGroup([FromBody] AnswerGroupUpdateModel model)
	{
		await _groupRepo.UpdateAsync(model);
		return ApiOk(new
		{
			success = true
		});
	}

	[HttpGet("list")]
	public async Task<IActionResult> ListAnswers(int answer_group_id)
	{
		return ApiOk(await _answerRepo.GetByGroupAsync(answer_group_id));
	}

	[HttpPost("create")]
	public async Task<IActionResult> CreateAnswer([FromBody] AnswerCreateModel model)
	{
		model.updated_by = GetUserId();
		return ApiOk(await _answerRepo.CreateAsync(model));
	}

	[HttpPost("update")]
	public async Task<IActionResult> UpdateAnswer([FromBody] AnswerUpdateModel model)
	{
		model.updated_by = GetUserId();
		await _answerRepo.UpdateAsync(model);
		return ApiOk(new
		{
			success = true
		});
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete([FromBody] DeleteDto dto)
	{
		await _answerRepo.DeleteAnswerAsync(dto.id);
		return ApiOk();
	}
}

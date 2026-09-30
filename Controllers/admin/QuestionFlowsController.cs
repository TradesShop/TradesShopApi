using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Repositories.Questions;

namespace TradePlatform.Api.Controllers.admin;

[Route("api/admin/[controller]")]
[ApiController]
public class QuestionFlowsController : BaseController
{
	private readonly IQuestionsRepository _repo;

	public QuestionFlowsController(IQuestionsRepository repo)
	{
		_repo = repo;
	}

	[HttpGet("list")]
	public async Task<IActionResult> List([FromQuery] int question_id)
	{
		return ApiOk(await _repo.GetFlowsAsync(question_id));
	}

	[HttpPost("create")]
	public async Task<IActionResult> Create([FromBody] QuestionFlowCreateDto qfcDto)
	{
		return ApiOk(await _repo.CreateFlowAsync(qfcDto));
	}

	[HttpPost("update")]
	public async Task<IActionResult> Update([FromBody] QuestionFlowUpdateDto qfuDto)
	{
		return ApiOk(await _repo.UpdateFlowAsync(qfuDto));
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete([FromBody] QuestionFlowDeleteDto dto)
	{
		await _repo.DeleteFlowAsync(dto.id);
		return ApiOk();
	}
}

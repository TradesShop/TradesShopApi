using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Repositories.Questions;

namespace TradePlatform.Api.Controllers.admin;

[Route("api/admin/[controller]")]
[ApiController]
public class QuestionsController : BaseController
{
	private readonly IQuestionsRepository _repo;

	public QuestionsController(IQuestionsRepository repo)
	{
		_repo = repo;
	}

	[HttpGet("list")]
	public async Task<IActionResult> List()
	{
		return ApiOk(await _repo.GetAllAsync());
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> Get(int id)
	{
		return ApiOk(await _repo.GetByIdAsync(id));
	}

	[HttpPost("create")]
	public async Task<IActionResult> Create([FromBody] QuestionCreateDto qcDto)
	{
		qcDto.updated_by = GetUserId();
		return ApiOk(await _repo.CreateAsync(qcDto));
	}

	[HttpPost("update")]
	public async Task<IActionResult> Update([FromBody] QuestionUpdateDto quDto)
	{
		quDto.updated_by = GetUserId();
		return ApiOk(await _repo.UpdateAsync(quDto));
	}
}

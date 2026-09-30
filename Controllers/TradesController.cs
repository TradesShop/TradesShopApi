using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradesController : BaseController
{
	private readonly ITradesRepository _repo;

	public TradesController(ITradesRepository repo)
	{
		_repo = repo;
	}

	[HttpGet]
	public async Task<IActionResult> GetTrades()
	{
		return ApiOk(await _repo.GetTradesAsync(null));
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetByTrade(int id)
	{
		return ApiOk(await _repo.GetTradesAsync(id));
	}
}

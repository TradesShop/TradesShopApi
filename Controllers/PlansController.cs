using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.plans;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlansController : BaseController
{
	private readonly PlansService _plansService;

	public PlansController(PlansService plansService)
	{
		_plansService = plansService;
	}

	[HttpGet]
	public async Task<IActionResult> GetActivePlans([FromQuery] string plan_type)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		return ApiOk(await _plansService.GetActivePlansAsync(plan_type, user_id));
	}

	[HttpGet("list")]
	public async Task<IActionResult> GetPlansListAsync([FromQuery] string? searchname, [FromQuery] string? type)
	{
		return ApiOk(await _plansService.GetPlansListAsync(searchname, type));
	}

	[HttpGet("checkname")]
	public async Task<IActionResult> PlansExistsAsync([FromQuery] string name, [FromQuery] Guid? exclude_id = null)
	{
		return ApiOk(await _plansService.PlansExistsAsync(name, exclude_id));
	}
}

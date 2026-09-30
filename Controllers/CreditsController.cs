using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Credits;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreditsController : BaseController
{
	private readonly ICreditService _creditService;

	public CreditsController(ICreditService creditService)
	{
		_creditService = creditService;
	}

	[HttpGet("my")]
	public async Task<IActionResult> MyCreditsAsync()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		Guid user_id = callerId;
		return ApiOk(await _creditService.GetBalanceAsync(user_id));
	}

	[HttpPost("history")]
	public async Task<IActionResult> MyCreditTranHistoryAsync([FromBody] credit_history_request req)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		req.user_id = ResolveEffectiveUser(user_id, user_type, req?.target_user_id);
		return ApiOk(await _creditService.MyCreditTranHistoryAsync(req));
	}

	[HttpPost("orders")]
	public async Task<IActionResult> GetCreditOrdersListAsync([FromBody] CreditOrdersSearch cosDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		cosDto.user_id = ResolveEffectiveUser(user_id, user_type, cosDto.target_user_id);
		return ApiOk(await _creditService.GetCreditOrdersListAsync(cosDto));
	}
}

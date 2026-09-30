using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Bundles;
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
		(Guid user_id, UserType userType) = GetIdentity();
		
		return ApiOk(await _creditService.GetBalanceAsync(user_id));
	}

	[HttpPost("history")]
	public async Task<IActionResult> MyCreditTranHistoryAsync([FromBody] credit_history_request req)
	{
		(Guid user_id, UserType user_type) = GetIdentity();		
		req.user_id = ResolveEffectiveUser(user_id, user_type, req?.target_user_id);
		return ApiOk(await _creditService.MyCreditTranHistoryAsync(req));
	}

	[HttpPost("orders")]
	public async Task<IActionResult> GetCreditOrdersListAsync([FromBody] CreditOrdersSearch cosDto)
	{
		(Guid user_id, UserType user_type) = GetIdentity();	
		cosDto.user_id = ResolveEffectiveUser(user_id, user_type, cosDto.target_user_id);
		return ApiOk(await _creditService.GetCreditOrdersListAsync(cosDto));
	}
    [HttpPost("cancel-without-refund")]
    public async Task<IActionResult> CreditOrderUpdateAsync([FromBody] CreditOrderUpdateDto couReq)
    {
        (Guid user_id, UserType user_type) = GetIdentity();
        couReq.user_id = ResolveEffectiveUser(user_id, user_type, couReq?.target_user_id);
		await _creditService.CreditOrderUpdateAsync(couReq);
        return ApiOk();
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Services.Subscriptions;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
public class AdminSubscriptionsController : BaseController
{
	private readonly IUserSubscriptionService _subsService;

	public AdminSubscriptionsController(IUserSubscriptionService subsService)
	{
		_subsService = subsService;
	}

	[HttpPost("cancel-scheduled-only")]
	public async Task<IActionResult> CancelScheduledOnly([FromBody] AdminSubscriptionCancelRequest req)
	{
		if (req == null || req.target_user_id == Guid.Empty)
		{
			return ApiError("Target user ID is required.");
		}
		_ = req.target_user_id;
		return ApiOk(await _subsService.CancelScheduledSubscriptionOnly(req.target_user_id, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("cancel-immediately")]
	public async Task<IActionResult> CancelImmediately([FromBody] AdminSubscriptionCancelRequest req)
	{
		if (req == null || req.target_user_id == Guid.Empty)
		{
			return ApiError("Target user ID is required.");
		}
		Guid effectiveUserId = req.target_user_id;
		return ApiOk(await _subsService.CancelSubscriptionImmediately(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("cancel-period-end")]
	public async Task<IActionResult> CancelAtPeriodEnd([FromBody] AdminSubscriptionCancelRequest req)
	{
		if (req == null || req.target_user_id == Guid.Empty)
		{
			return ApiError("Target user ID is required.");
		}
		Guid effectiveUserId = req.target_user_id;
		return ApiOk(await _subsService.CancelSubscriptionEndOfPeriod(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("auto-renewal")]
	public async Task<IActionResult> SetSubscriptionAutoRenewal([FromBody] AdminSubscriptionCancelRequest req)
	{
		if (req == null || req.target_user_id == Guid.Empty)
		{
			return ApiError("Target user ID is required.");
		}
		Guid effectiveUserId = req.target_user_id;
		return ApiOk(await _subsService.SetSubscriptionAutoRenewal(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}
}

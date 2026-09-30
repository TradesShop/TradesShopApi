using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Subscriptions;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : BaseController
{
	public class CreateSubscriptionRequest
	{
		public string price_id { get; set; }
	}

	private readonly IUserSubscriptionService _subsService;

	private Guid UserId => Guid.Parse(User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));

	public SubscriptionsController(IUserSubscriptionService subsService)
	{
		_subsService = subsService;
	}

	[HttpPost("active")]
	public async Task<IActionResult> GetActive([FromBody] SubscriptionActiveReq saReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid user_id = ResolveEffectiveUser(callerId, callerType, saReq?.target_user_id);
		return ApiOk(await _subsService.GetActiveSubscriptionForUserAsync(user_id, saReq.plan_type));
	}

	[HttpPost("select")]
	public async Task<IActionResult> SelectSubscriptionAsync([FromBody] SubscriptionSelectRequest req)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, req?.target_user_id);
		return ApiOk(await _subsService.SelectSubscriptionAsync(effectiveUserId, req.plan_id, req.plan_price_id));
	}

	[HttpPost("cancel-scheduled-only")]
	public async Task<IActionResult> CancelScheduledOnly([FromBody] SubscriptionCancelRequest req)
	{
		if (req == null)
		{
			return ApiError("Invalid request");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(user_id, user_type, req.target_user_id);
		return ApiOk(await _subsService.CancelScheduledSubscriptionOnly(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("cancel-immediately")]
	public async Task<IActionResult> CancelImmediately([FromBody] SubscriptionCancelRequest req)
	{
		if (req == null)
		{
			return ApiError("Invalid request");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(user_id, user_type, req.target_user_id);
		return ApiOk(await _subsService.CancelSubscriptionImmediately(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("cancel-period-end")]
	public async Task<IActionResult> CancelAtPeriodEnd([FromBody] SubscriptionCancelRequest req)
	{
		if (req == null)
		{
			return ApiError("Invalid request");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(user_id, user_type, req.target_user_id);
		return ApiOk(await _subsService.CancelSubscriptionEndOfPeriod(effectiveUserId, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("auto-renewal")]
	public async Task<IActionResult> SetSubscriptionAutoRenewal([FromBody] SubscriptionCancelRequest req)
	{
		if (req == null)
		{
			return ApiError("Invalid request");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		return ApiOk(await _subsService.SetSubscriptionAutoRenewal(user_id, req.plan_price_id, req.stripe_subscription_id));
	}

	[HttpPost("list")]
	public async Task<IActionResult> SubscriptionsListAsync([FromBody] CommonSearchReq csreq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		csreq.target_user_id = ResolveEffectiveUser(user_id, user_type, csreq?.target_user_id);
		return ApiOk(await _subsService.SubscriptionsListAsync(csreq));
	}

	[HttpPost("history")]
	public async Task<IActionResult> SubscriptionsHistoryAsync([FromBody] CommonSearchReq csreq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		csreq.target_user_id = ResolveEffectiveUser(user_id, user_type, csreq?.target_user_id);
		return ApiOk(await _subsService.SubscriptionsHistoryAsync(csreq));
	}

	[HttpPost("scheduled")]
	public async Task<IActionResult> ScheduledSubscriptionsAsync([FromBody] subscriptionpending_req spreq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		spreq.user_id = ResolveEffectiveUser(callerId, callerType, spreq?.user_id);
		return ApiOk(await _subsService.ScheduledSubscriptionsAllAsync(spreq));
	}
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : BaseController
{
	private readonly IPaymentsRepository _payservice;

	private readonly IStripeService _stripeservice;

	private readonly StripeClient _stripe;

	private readonly IPaymentMethodRepository _paymentMethods;

	private readonly IUsersRepository _users;

	private new readonly IIdentityService _identity;

	private readonly IPaymentTshIntentService _paymentIntentService;

	public PaymentsController(IPaymentsRepository payservice, StripeClient stripe, IPaymentMethodRepository paymentMethods, IUsersRepository users, IStripeService stripeservice, IIdentityService identity, IPaymentTshIntentService paymentIntentService)
	{
		_payservice = payservice;
		_stripe = stripe;
		_paymentMethods = paymentMethods;
		_users = users;
		_stripeservice = stripeservice;
		_identity = identity;
		_paymentIntentService = paymentIntentService;
	}

	private new Guid ResolveEffectiveUser(Guid callerId, UserType callerType, Guid? targetUserId)
	{
		if (callerType != UserType.admin || !targetUserId.HasValue)
		{
			return callerId;
		}
		return targetUserId.Value;
	}

	[HttpPost("setup-intent")]
	public async Task<IActionResult> CreateSetupIntent([FromBody] SetupIntentDto? request)
	{
		var (callerId, callerType) = _identity.GetIdentity();
		return ApiOk(await _payservice.CreateSetupIntentAsync(callerId, callerType, request?.target_user_id));
	}

	[HttpPost("attach")]
	public async Task<IActionResult> AttachPaymentMethod([FromBody] AttachPaymentMethodDto attach_dto)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, attach_dto?.target_user_id);
		return Ok(await _payservice.AttachPaymentMethodAsync(effectiveUserId, callerType, attach_dto.payment_method_id));
	}

	[HttpGet("methods")]
	public async Task<IActionResult> GetMethods([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, target_user_id);
		return ApiOk(await _paymentMethods.GetPaymentMethodsAsync(effectiveUserId));
	}

	[HttpGet("methods/default")]
	public async Task<IActionResult> GetDefaultPaymentMethod([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(user_id, user_type, target_user_id);
		return ApiOk(await _paymentMethods.GetDefaultPaymentMethodAsync(effectiveUserId));
	}

	[HttpPost("methods/default/{id}")]
	public async Task<IActionResult> SetDefaultCard(string id, [FromBody] SetDefaultPaymentMethodDto? pmDto)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, pmDto?.target_user_id);
		await _stripeservice.SetDefaultPaymentMethodAsync(effectiveUserId, id);
		return ApiOk();
	}

	[HttpPut("methods/detach/{id}")]
	public async Task<IActionResult> DetachCard(string id, [FromBody] DetachPaymentMethodDto? pmDto)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, pmDto?.target_user_id);
		await _stripeservice.DetachPaymentMethodAsync(effectiveUserId, id);
		return ApiOk();
	}

	[HttpPost("subscribe")]
	public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
	{
		var (callerId, callerType) = _identity.GetIdentity();
		return Ok(await _payservice.SubscribeAsync(callerId, callerType, request.priceid, request.paymentmethodid, request.targetuserid));
	}

	[HttpPost("cancel-subscription")]
	public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionDto request)
	{
		var (userId, userType) = _identity.GetIdentity();
		await _payservice.CancelSubscriptionAsync(userId, userType, request.stripe_subscription_id, request.targetuserid);
		return ApiOk();
	}

	[HttpPut("methods/update/{id}")]
	public async Task<IActionResult> UpdatePaymentMethod([FromRoute] string id, [FromBody] PaymentMethodUpdateDto dto)
	{
		(Guid userId, UserType userType) identity = _identity.GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, dto.target_user_id);
		await _stripeservice.UpdatePaymentMethodAsync(id, dto.name_on_card, dto.exp_month, dto.exp_year);
		await _paymentMethods.UpdatePaymentMethodAsync(id, dto.name_on_card, dto.exp_month, dto.exp_year, effectiveUserId);
		return ApiOk();
	}
}

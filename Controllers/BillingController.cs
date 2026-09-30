using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BillingController : BaseController
{
	private readonly IBillingServices _billingService;

	private readonly IPaymentMethodRepository _paymentMethods;

	private readonly IPaymentsRepository _payservice;

	private readonly ITdsInvoiceService _invservice;

	private readonly IRefundServices _refService;

	public BillingController(IBillingServices billingService, IPaymentMethodRepository paymentMethod, IPaymentsRepository payservice, ITdsInvoiceService invservice, IRefundServices refService)
	{
		_billingService = billingService;
		_paymentMethods = paymentMethod;
		_payservice = payservice;
		_invservice = invservice;
		_refService = refService;
	}

	[HttpPost("attach")]
	public async Task<IActionResult> AttachPaymentMethod([FromBody] AttachPaymentMethodDto attach_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, attach_dto?.target_user_id);
		return ApiOk(await _payservice.AttachPaymentMethodAsync(effectiveUserId, callerType, attach_dto.payment_method_id));
	}

	[HttpGet("methods")]
	public async Task<IActionResult> GetMethods([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, target_user_id);
		return ApiOk(await _paymentMethods.GetPaymentMethodsAsync(effectiveUserId));
	}

	[HttpPost("invoices")]
	public async Task<IActionResult> GetInvoicesList([FromBody] InvoiceListRequest invReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		invReq.user_id = ResolveEffectiveUser(user_id, user_type, invReq.target_user_id);
		return ApiOk(await _invservice.GetInvoiceListAsync(invReq));
	}

	[HttpPost("refunds")]
	public async Task<IActionResult> GetChargesList([FromBody] RefundListRequest refReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		refReq.user_id = ResolveEffectiveUser(user_id, user_type, refReq.target_user_id);
		return ApiOk(await _refService.GetRefundListAsync(refReq));
	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Payments;
using TradePlatform.Api.Services.stripe;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RefundController : BaseController
{
	private readonly IRefundServices _refundService;

	private readonly IStripeRefundService _stripeRefund;

	public RefundController(IRefundServices refundService, IStripeRefundService stripeRefund)
	{
		_refundService = refundService;
		_stripeRefund = stripeRefund;
	}

	[HttpGet("stripe-reasons")]
	public IActionResult GetRefundReasons()
	{
		return ApiOk(new[]
		{
			new
			{
				id = "duplicate",
				code = "duplicate",
				name = "Duplicate payment"
			},
			new
			{
				id = "fraudulent",
				code = "fraudulent",
				name = "Fraudulent payment"
			},
			new
			{
				id = "requested_by_customer",
				code = "requested_by_customer",
				name = "Requested by customer"
			},
			new
			{
				id = (string)null,
				code = (string)null,
				name = "Unknown"
			}
		});
	}

	[HttpPost("process")]
	public async Task<IActionResult> RefundProcess(Guid refund_id, Guid refund_request_id)
	{
		return ApiOk(await _refundService.RefundProcessAsync(refund_id, refund_request_id));
	}

	[HttpPost("create")]
	public async Task<IActionResult> CreateRefund([FromBody] RefundCreateRequest request)
	{
		if (request == null)
		{
			return ApiError("Invalid request");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		request.updated_by = user_id;
		RefundViewDto refund = await _refundService.CreateRefundAsync(request);
		if (refund == null || refund.id == Guid.Empty)
		{
			int[] unapprovedStatuses = new int[6] { 71, 72, 74, 75, 76, 77 };
			if (request.refund_req_status_id.HasValue && Enumerable.Contains(unapprovedStatuses, request.refund_req_status_id.Value))
			{
				return ApiOk(new
				{
					success = true,
					is_processed = false,
					action_type = "request_updated_only",
					message = "Your refund request was updated successfully. The refund has not been processed because the request has not been approved."
				});
			}
			return ApiError("Unable to create the refund.");
		}
		return ApiOk(await _refundService.RefundProcessAsync(refund.id, refund.refund_request_id));
	}

	[HttpPost("request")]
	public async Task<IActionResult> CreateRefundRequest([FromBody] RefundRequestCreateDto rrc_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		rrc_dto.user_id = ResolveEffectiveUser(user_id, user_type, rrc_dto.target_user_id);
		rrc_dto.created_by = user_id;
		RefundRequestResponse refundreq = await _refundService.CreateRefundRequestAsync(rrc_dto);
		if (refundreq == null)
		{
			return ApiError(new
			{
				error = "Refund request failed."
			});
		}
		return ApiOk(refundreq);
	}

	[HttpPost("requests/list")]
	public async Task<IActionResult> RefundRequestList([FromBody] RefundsReqSearch rrsreq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		_ = identity.userId;
		_ = identity.userType;
		return ApiOk(await _refundService.GetRefundRequestListAsync(rrsreq));
	}

	[HttpPost("calculate")]
	public async Task<IActionResult> GetRefundCalculationAsync([FromBody] RefundRequestCreateDto rrc_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		rrc_dto.user_id = ResolveEffectiveUser(user_id, user_type, rrc_dto.target_user_id);
		rrc_dto.created_by = user_id;
		RefundCalculationResponse result = await _refundService.GetRefundCalculationAsync(rrc_dto);
		if (result == null)
		{
			return ApiError(new
			{
				error = "Refund request failed."
			});
		}
		return ApiOk(result);
	}

    [HttpPost("request-without-refund")]
    public async Task<IActionResult> CreateRequestWithoutRefund([FromBody] RefundRequestCreateDto rrc_dto)
    {
        (Guid userId, UserType userType) identity = GetIdentity();
        Guid user_id = identity.userId;
        UserType user_type = identity.userType;
        rrc_dto.user_id = ResolveEffectiveUser(user_id, user_type, rrc_dto.target_user_id);
        rrc_dto.created_by = user_id;
        RefundRequestResponse refundreq = await _refundService.CreateRefundRequestAsync(rrc_dto);
        if (refundreq == null)
        {
            return ApiError(new
            {
                error = "Refund request failed."
            });
        }
        return ApiOk(refundreq);
    }
}

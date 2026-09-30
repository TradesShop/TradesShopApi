using System;
using System.Threading.Tasks;
using Stripe;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.stripe;

namespace TradePlatform.Api.Services.Payments;

public class RefundServices : IRefundServices
{
	private readonly IRefundRepository _refundRepo;

	private readonly IStripeRefundService _stripeRefund;

	private readonly IIdentityService _identityService;

	public RefundServices(IRefundRepository refundRepo, IStripeRefundService stripeRefund, IIdentityService identityService)
	{
		_refundRepo = refundRepo;
		_stripeRefund = stripeRefund;
		_identityService = identityService;
	}

	public async Task<RefundRequestResponse> CreateRefundRequestAsync(RefundRequestCreateDto rrc_dto)
	{
		return await _refundRepo.CreateRefundRequestAsync(rrc_dto);
	}

	public async Task<RefundCalculationResponse?> GetRefundCalculationAsync(RefundRequestCreateDto rrc_dto)
	{
		return await _refundRepo.GetRefundCalculationAsync(rrc_dto);
	}

	public async Task<RefundRequestListResult> GetRefundRequestListAsync(RefundsReqSearch rrsreq)
	{
		return await _refundRepo.GetRefundRequestListAsync(rrsreq);
	}

	public async Task<RefundViewDto> CreateRefundAsync(RefundCreateRequest request)
	{
		return await _refundRepo.CreateRefundAsync(request);
	}

	public async Task<RefundRequestResponse> GetRefundRequestById(Guid refund_request_id)
	{
		return await _refundRepo.GetRefundRequestById(refund_request_id);
	}

	public async Task<RefundViewDto?> GetRefundViewById(Guid refund_id)
	{
		return await _refundRepo.GetRefundViewById(refund_id);
	}

	public async Task<RefundViewDto?> UpdateRefundAsync(RefundUpdateRequest request)
	{
		return await _refundRepo.UpdateRefundAsync(request);
	}

	public async Task<RefundViewDto?> RefundProcessAsync(Guid refund_id, Guid refund_request_id)
	{
		RefundViewDto refundRequest = await GetRefundViewById(refund_id);
		if (refundRequest == null)
		{
			throw new InvalidOperationException("Refund request not exists");
		}
		if (refundRequest.refund_req_status_id == 75)
		{
			throw new InvalidOperationException("Refund already processed");
		}
		Refund stripeRefund = await _stripeRefund.CreateStripeRefundAsync(refundRequest, refund_id);
		return await UpdateRefundAsync(new RefundUpdateRequest
		{
			refund_id = refund_id,
			status_id = 82,
			stripe_refund_id = stripeRefund.Id,
			stripe_refund_status = stripeRefund.Status,
			comments = "Refund completed successfully by Stripe",
			updated_by = _identityService.GetCurrentUserId()
		});
	}

	public async Task<RefundListResponse> GetRefundListAsync(RefundListRequest invreq)
	{
		return await _refundRepo.GetRefundListAsync(invreq);
	}
}

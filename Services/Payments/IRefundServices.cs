using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Refunds;

namespace TradePlatform.Api.Services.Payments;

public interface IRefundServices
{
	Task<RefundRequestResponse> CreateRefundRequestAsync(RefundRequestCreateDto rrc_dto);

	Task<RefundCalculationResponse?> GetRefundCalculationAsync(RefundRequestCreateDto rrc_dto);

	Task<RefundRequestListResult> GetRefundRequestListAsync(RefundsReqSearch rrsreq);

	Task<RefundViewDto> CreateRefundAsync(RefundCreateRequest request);

	Task<RefundRequestResponse> GetRefundRequestById(Guid refund_request_id);

	Task<RefundViewDto?> UpdateRefundAsync(RefundUpdateRequest request);

	Task<RefundViewDto?> RefundProcessAsync(Guid refund_id, Guid refund_request_id);

	Task<RefundViewDto?> GetRefundViewById(Guid refund_id);

	Task<RefundListResponse> GetRefundListAsync(RefundListRequest invreq);
}

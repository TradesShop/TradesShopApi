using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IRefundRepository
{
	Task<RefundRequestResponse?> CreateRefundRequestAsync(RefundRequestCreateDto rrc_dto);

	Task<RefundRequestListResult> GetRefundRequestListAsync(RefundsReqSearch rrsreq);

	Task<RefundCalculationResponse?> GetRefundCalculationAsync(RefundRequestCreateDto dto);

	Task<RefundViewDto?> CreateRefundAsync(RefundCreateRequest request);

	Task<RefundRequestResponse?> GetRefundRequestById(Guid refund_request_id);

	Task<RefundViewDto?> UpdateRefundAsync(RefundUpdateRequest request);

	Task<RefundViewDto?> GetRefundViewById(Guid refund_id);

	Task<RefundListResponse> GetRefundListAsync(RefundListRequest refReq);

	Task<BillingNotifyEmailDataModel> RefundSucceededEmailDetails(Guid refund_id, Guid entity_id, int entity_type_id);
}

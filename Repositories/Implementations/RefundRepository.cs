using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.BackgroundEmailQueue;

namespace TradePlatform.Api.Repositories.Implementations;

public class RefundRepository : IRefundRepository
{
	private readonly DapperContext _context;

	private readonly IBackgroundEmailQueue _backgroundEmailQueue;

	public RefundRepository(DapperContext context, IBackgroundEmailQueue backgroundEmailQueue)
	{
		_context = context;
		_backgroundEmailQueue = backgroundEmailQueue;
	}

	public async Task<RefundCalculationResponse?> GetRefundCalculationAsync(RefundRequestCreateDto dto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		return await connection.QueryFirstOrDefaultAsync<RefundCalculationResponse>(" SELECT * FROM [dbo].[ufn_refund_calculate_amount](\r\n                    @entity_type_id,\r\n                    @entity_id\r\n                );", new { dto.entity_type_id, dto.entity_id });
	}

	public async Task<RefundRequestResponse?> CreateRefundRequestAsync(RefundRequestCreateDto rrc_dto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { rrc_dto.user_id, rrc_dto.entity_type_id, rrc_dto.entity_id, rrc_dto.reason, rrc_dto.created_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<RefundRequestResponse>("[dbo].[usp_refund_request_create]", param, null, null, commandType);
	}

	public async Task<RefundRequestResponse?> GetRefundRequestById(Guid refund_request_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			id = refund_request_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<RefundRequestResponse>("[dbo].[usp_refund_request_get_by_id]", param, null, null, commandType);
	}

	public async Task<RefundViewDto?> GetRefundViewById(Guid refund_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			id = refund_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<RefundViewDto>("[dbo].[usp_refund_get_by_id]", param, null, null, commandType);
	}

	public async Task<RefundRequestListResult> GetRefundRequestListAsync(RefundsReqSearch rrsreq)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		new DynamicParameters();
		string statusCsv = ((rrsreq.status_ids != null && rrsreq.status_ids.Any()) ? string.Join(",", rrsreq.status_ids) : null);
		var param = new
		{
			status_ids = statusCsv,
			date_from = rrsreq.date_from,
			date_to = rrsreq.date_to,
			page_number = rrsreq.page_number,
			page_size = rrsreq.page_size
		};
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader RefundReqList = await connection.QueryMultipleAsync("[dbo].[usp_refund_requests_list]", param, null, null, commandType);
		RefundRequestListResult AnyRefundReq = new RefundRequestListResult();
		AnyRefundReq.items = RefundReqList.Read<RefundRequestListDto>().ToList();
		AnyRefundReq.total_records = RefundReqList.ReadSingle<int>();
		return AnyRefundReq;
	}

	public async Task<RefundViewDto?> CreateRefundAsync(RefundCreateRequest request)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { request.refund_request_id, request.refund_net, request.refund_vat, request.refund_gross, request.currency, request.refund_req_status_id, request.stripe_refund_reason, request.notes, request.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<RefundViewDto>("[dbo].[usp_refund_create]", param, null, null, commandType);
	}

	public async Task<RefundViewDto?> UpdateRefundAsync(RefundUpdateRequest request)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { request.refund_id, request.status_id, request.stripe_refund_id, request.stripe_failure_reason, request.stripe_refund_status, request.comments, request.receipt_url, request.updated_by };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<RefundViewDto>("[dbo].[usp_refund_update]", param, null, null, commandType);
	}

	public async Task<RefundListResponse> GetRefundListAsync(RefundListRequest refReq)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { refReq.user_id, refReq.page_number, refReq.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader RefundList = await connection.QueryMultipleAsync("[dbo].[usp_refunds_list]", param, null, null, commandType);
		RefundListResponse RefundListRes = new RefundListResponse();
		RefundListRes.items = RefundList.Read<RefundListItem>().ToList();
		RefundListRes.total_records = RefundList.ReadSingle<int>();
		return RefundListRes;
	}

	public async Task<BillingNotifyEmailDataModel> RefundSucceededEmailDetails(Guid refund_id, Guid entity_id, int entity_type_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			id = refund_id,
			entity_id = entity_id,
			entity_type_id = entity_type_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<BillingNotifyEmailDataModel>("[dbo].[usp_refund_succeeded_email_details]", param, null, null, commandType);
	}
}

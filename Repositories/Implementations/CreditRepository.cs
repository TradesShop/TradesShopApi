using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TradePlatform.Api.Repositories.Implementations;

public class CreditRepository : ICreditRepository
{
	private readonly DapperContext _context;

	public CreditRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task GrantAsync(CreditGrantRequest request)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { request.user_id, request.source, request.reference_id, request.total_credits, request.expires_at, request.reference_type, request.metadata };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_credit_grant_create", param, null, null, commandType);
	}

	public async Task ConsumeAsync(CreditConsumeRequest request)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { request.user_id, request.credits_to_use, request.reference_type, request.reference_id, request.metadata };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_credit_consume_fifo", param, null, null, commandType);
	}

	public async Task RefundAsync(CreditRefundRequest request)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { request.user_id, request.credits_to_refund, request.reference_type, request.reference_id, request.expires_at, request.metadata };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_credit_refund", param, null, null, commandType);
	}

	public async Task<int> GetBalanceAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.ExecuteScalarAsync<int>("usp_credit_get_balance", param, null, null, commandType);
	}

	public async Task<CreditTransactionsHistoryResult> MyCreditTranHistoryAsync(credit_history_request crdh_req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { crdh_req.user_id, crdh_req.page_number, crdh_req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader tran_result = await conn.QueryMultipleAsync("usp_credit_transactions_history", param, null, null, commandType);
		CreditTransactionsHistoryResult TransactionHistory = new CreditTransactionsHistoryResult();
		TransactionHistory.items = tran_result.Read<TransactionItem>().ToList();
		TransactionHistory.total_records = tran_result.ReadSingle<int>();
		return TransactionHistory;
	}

	public async Task<CreditOrdersListResult> GetCreditOrdersListAsync(CreditOrdersSearch cosDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { cosDto.user_id, cosDto.page_number, cosDto.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader creditorders = await connection.QueryMultipleAsync("[dbo].[usp_credit_bundle_orders_list]", param, null, null, commandType);
		CreditOrdersListResult CreditOrdersList = new CreditOrdersListResult();
		CreditOrdersList.items = creditorders.Read<CreditOrderItem>().ToList();
		CreditOrdersList.total_records = creditorders.ReadSingle<int>();
		return CreditOrdersList;
	}

    public async Task CreditOrderUpdateAsync(CreditOrderUpdateDto couReq)
    {
        using IDbConnection conn = _context.CreateOpenConnection();
        var param = new { 
			 id=couReq.id
			,is_refund_requested = couReq.is_refund_requested
            ,cancellation_reason=couReq.cancellation_reason
            ,actor = couReq.actor
            ,source= couReq.source		    
			,metadata_json= couReq.metadata_json
        };     
        await conn.ExecuteAsync("[dbo].[usp_bundle_order_cancel_request]", param);
    }
}

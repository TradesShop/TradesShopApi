using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Credits;

public class CreditService : ICreditService
{
	private readonly ICreditRepository _credits;

	public CreditService(ICreditRepository credits)
	{
		_credits = credits;
	}

	public Task GrantAsync(CreditGrantRequest request)
	{
		return _credits.GrantAsync(request);
	}

	public Task ConsumeAsync(CreditConsumeRequest request)
	{
		return _credits.ConsumeAsync(request);
	}

	public Task RefundAsync(CreditRefundRequest request)
	{
		return _credits.RefundAsync(request);
	}

	public Task<int> GetBalanceAsync(Guid user_id)
	{
		return _credits.GetBalanceAsync(user_id);
	}

	public async Task<CreditTransactionsHistoryResult> MyCreditTranHistoryAsync(credit_history_request crdh_req)
	{
		return await _credits.MyCreditTranHistoryAsync(crdh_req);
	}

	public async Task<CreditOrdersListResult> GetCreditOrdersListAsync(CreditOrdersSearch cosDto)
	{
		return await _credits.GetCreditOrdersListAsync(cosDto);
	}
    public async Task CreditOrderUpdateAsync(CreditOrderUpdateDto couReq)
    {
        await _credits.CreditOrderUpdateAsync(couReq);
    }
}

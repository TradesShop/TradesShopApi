using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Credits;

namespace TradePlatform.Api.Services.Credits;

public interface ICreditService
{
	Task GrantAsync(CreditGrantRequest request);

	Task ConsumeAsync(CreditConsumeRequest request);

	Task RefundAsync(CreditRefundRequest request);

	Task<int> GetBalanceAsync(Guid user_id);

	Task<CreditTransactionsHistoryResult> MyCreditTranHistoryAsync(credit_history_request crdh_req);

	Task<CreditOrdersListResult> GetCreditOrdersListAsync(CreditOrdersSearch cosDto);
}

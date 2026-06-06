using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Credits
{
    public class CreditService : ICreditService
    {
        private readonly ICreditRepository _credits;

        public CreditService(ICreditRepository credits)
        {
            _credits = credits;
        }

        // Subscription / bundle / promotion / admin
        public Task GrantAsync(CreditGrantRequest request) => _credits.GrantAsync(request);

        // Job usage
        public Task ConsumeAsync(CreditConsumeRequest request) => _credits.ConsumeAsync(request);

        // Dispute refund
        public Task RefundAsync(CreditRefundRequest request) => _credits.RefundAsync(request);

        public Task<int> GetBalanceAsync(Guid user_id) => _credits.GetBalanceAsync(user_id);
    }

   
}

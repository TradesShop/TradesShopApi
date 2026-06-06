using TradePlatform.Api.DTOs.Credits;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ICreditRepository
    {
        Task GrantAsync(CreditGrantRequest request);
        Task ConsumeAsync(CreditConsumeRequest request);
        Task RefundAsync(CreditRefundRequest request);
        Task<int> GetBalanceAsync(Guid user_id);
    }
}

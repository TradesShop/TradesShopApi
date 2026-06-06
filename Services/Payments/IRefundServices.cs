using TradePlatform.Api.DTOs.Invoices;

namespace TradePlatform.Api.Services.Payments
{
    public interface IRefundServices
    {
        Task<bool> RefundPaymentAsync(RefundRequestDto dto);
    }
}

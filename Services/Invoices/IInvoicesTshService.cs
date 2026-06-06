using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Invoices;

namespace TradePlatform.Api.Services.Invoices
{
    public interface IInvoicesTshService
    {
        //Task<InvoiceDto?> GetInvoiceAsync(Guid invoice_id);
        //Task<InvoiceDto?> GetInvoiceByStripeInvoiceIdAsync(string stripe_invoice_id);
        //Task<InvoiceDto> CreateInvoiceAsync(InvoiceEventProcessDto dto);
        Task MarkInvoicePaidAsync(Guid invoice_id, DateTime paid_at);
        Task MarkInvoiceFailedAsync(Guid invoice_id);
        Task<PagedResultDto<InvoiceDto>> GetPagedAsync(InvoiceQueryDto query);
    }
}

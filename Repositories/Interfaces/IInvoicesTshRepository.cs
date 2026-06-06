using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IInvoicesTshRepository
    {
        Task Invoice_event_process_updateAsync(InvoiceEventProcessDto model);
        //Task<Invoices> InsertInvoiceAsync(Invoices invoice);
        // Webhook-related
        Task MarkPaidAsync(string stripe_invoice_id);
        Task MarkFailedAsync(string stripe_invoice_id);

        Task<Invoices?> GetByIdAsync(Guid invoice_id);
        Task<Invoices?> GetByStripeInvoiceIdAsync(string stripe_invoice_id);
        //Task CreateAsync(Invoices model);
        Task UpdateAsync(Invoices model);
        Task<PagedResultDto<Invoices>> GetPagedAsync(InvoiceQueryDto query);
        Task<IEnumerable<Invoices>> GetByUserAsync(Guid user_id);
        
    }
}

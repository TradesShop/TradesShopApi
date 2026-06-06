using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Helpers;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Invoices
{
    public class InvoicesTshService : IInvoicesTshService
    {
        private readonly IInvoicesTshRepository _invoicesRepo;
        private readonly IInvoiceItemsRepository _itemsRepo;

        public InvoicesTshService(
            IInvoicesTshRepository invoicesRepo,
            IInvoiceItemsRepository itemsRepo)
        {
            _invoicesRepo = invoicesRepo;
            _itemsRepo = itemsRepo;
        }

        //public async Task<InvoiceDto?> GetInvoiceAsync(Guid invoice_id)
        //{
        //    var invoice = await _invoicesRepo.GetByIdAsync(invoice_id);
        //    if (invoice == null) return null;

        //    var items = await _itemsRepo.GetByInvoiceIdAsync(invoice_id);
        //   // return InvoicesMappingHelper.ToDto(invoice, items);
        //}

        //public async Task<InvoiceDto?> GetInvoiceByStripeInvoiceIdAsync(string stripe_invoice_id)
        //{
        //    var invoice = await _invoicesRepo.GetByStripeInvoiceIdAsync(stripe_invoice_id);
        //    if (invoice == null) return null;

        //    var items = await _itemsRepo.GetByInvoiceIdAsync(invoice.id);
        //    return InvoicesMappingHelper.ToDto(invoice, items);
        //}

        

        public async Task MarkInvoicePaidAsync(Guid invoice_id, DateTime paid_at)
        {
            var invoice = await _invoicesRepo.GetByIdAsync(invoice_id);
            if (invoice == null) return;

            invoice.status = "paid";
            invoice.paid_at = paid_at;
            invoice.updated_at = DateTime.UtcNow;

            await _invoicesRepo.UpdateAsync(invoice);
        }

        public async Task MarkInvoiceFailedAsync(Guid invoice_id)
        {
            var invoice = await _invoicesRepo.GetByIdAsync(invoice_id);
            if (invoice == null) return;

            invoice.status = "failed";
            invoice.updated_at = DateTime.UtcNow;

            await _invoicesRepo.UpdateAsync(invoice);
        }
        public async Task<PagedResultDto<InvoiceDto>> GetPagedAsync(InvoiceQueryDto query)
        {
            var paged = await _invoicesRepo.GetPagedAsync(query);

            var result = new PagedResultDto<InvoiceDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                Items = new List<InvoiceDto>()
            };

            //foreach (var invoice in paged.Items)
            //{
            //    var items = await _itemsRepo.GetByInvoiceIdAsync(invoice.id);
            //    result.Items = result.Items.Append(InvoicesMappingHelper.ToDto(invoice, items));
            //}

            return result;
        }

    }
}

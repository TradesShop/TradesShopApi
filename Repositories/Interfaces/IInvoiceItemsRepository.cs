using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IInvoiceItemsRepository
    {
        Task<IEnumerable<InvoiceItems>> GetByInvoiceIdAsync(Guid invoice_id);
        //Task CreateManyAsync(IEnumerable<InvoiceItems> items);
    }
}

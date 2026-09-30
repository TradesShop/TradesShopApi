using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Invoices;

namespace TradePlatform.Api.Services;

public interface ITdsInvoiceService
{
	Task<InvoiceListResponse> GetInvoiceListAsync(InvoiceListRequest invreq);
}

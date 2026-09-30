using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services;

public class TdsInvoiceService : ITdsInvoiceService
{
	private readonly ITdsInvoiceRepository _tdsInvRepo;

	public TdsInvoiceService(ITdsInvoiceRepository tdsInvRepo)
	{
		_tdsInvRepo = tdsInvRepo;
	}

	public async Task<InvoiceListResponse> GetInvoiceListAsync(InvoiceListRequest invreq)
	{
		return await _tdsInvRepo.GetInvoiceListAsync(invreq);
	}
}

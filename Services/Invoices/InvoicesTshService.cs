using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Invoices;

public class InvoicesTshService : IInvoicesTshService
{
	private readonly ITdsInvoiceRepository _invoicesRepo;

	private readonly IInvoiceItemsRepository _itemsRepo;

	public InvoicesTshService(ITdsInvoiceRepository invoicesRepo, IInvoiceItemsRepository itemsRepo)
	{
		_invoicesRepo = invoicesRepo;
		_itemsRepo = itemsRepo;
	}
}

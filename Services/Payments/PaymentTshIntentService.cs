using Microsoft.Extensions.Configuration;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments;

public class PaymentTshIntentService : IPaymentTshIntentService
{
	private readonly ITdsInvoiceRepository _invoicesRepo;

	private readonly IInvoiceItemsRepository _itemsRepo;

	private readonly IConfiguration _config;

	public PaymentTshIntentService(ITdsInvoiceRepository invoicesRepo, IInvoiceItemsRepository itemsRepo, IConfiguration config)
	{
		_invoicesRepo = invoicesRepo;
		_itemsRepo = itemsRepo;
		_config = config;
	}
}

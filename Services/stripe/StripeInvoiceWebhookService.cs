using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Invoices;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Services.stripe;

public class StripeInvoiceWebhookService : IStripeInvoiceWebhookService
{
	private readonly ITdsInvoiceRepository _invoicesRepo;

	private readonly IInvoicesTshService _invoicesService;

	private readonly IPaymentsService _paymentsService;

	private readonly IPaymentsRepository _paymentsRepo;

	public StripeInvoiceWebhookService(ITdsInvoiceRepository invoicesRepo, IInvoicesTshService invoicesService, IPaymentsService paymentsService, IPaymentsRepository paymentsRepo)
	{
		_invoicesRepo = invoicesRepo;
		_invoicesService = invoicesService;
		_paymentsService = paymentsService;
		_paymentsRepo = paymentsRepo;
	}
}

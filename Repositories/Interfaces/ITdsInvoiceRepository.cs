using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface ITdsInvoiceRepository
{
	Task Invoice_event_process_updateAsync(InvoiceEventProcessDto model);

	Task MarkPaidAsync(string stripe_invoice_id);

	Task MarkFailedAsync(string stripe_invoice_id);

	Task<Invoices?> GetByIdAsync(Guid invoice_id);

	Task<Invoices?> GetByStripeInvoiceIdAsync(string stripe_invoice_id);

	Task<IEnumerable<Invoices>> GetByUserAsync(Guid user_id);

	Task<InvoiceListResponse> GetInvoiceListAsync(InvoiceListRequest invreq);

	Task InvoiceInsertOnCreated(InvoiceUpsertProcessDto model);

	Task InvoiceUpdateOnFinalized(InvoiceUpsertProcessDto model);

	Task InvoiceUpdateOnPaid(InvoiceUpsertProcessDto model);

	Task InvoiceUpdateOnStatusChanged(string stripe_invoice_id, string status);

	Task<BillingNotifyEmailDataModel> InvoicePaidNotifyEmailDetails(InvEventEmailNotifyDto InvPaidNotify);
}

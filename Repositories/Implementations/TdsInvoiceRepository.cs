using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.BackgroundEmailQueue;

namespace TradePlatform.Api.Repositories.Implementations;

public class TdsInvoiceRepository : ITdsInvoiceRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identityService;

	private readonly IBackgroundEmailQueue _backgroundEmailQueue;

	public TdsInvoiceRepository(DapperContext context, IIdentityService identityService, IBackgroundEmailQueue backgroundEmailQueue)
	{
		_context = context;
		_identityService = identityService;
		_backgroundEmailQueue = backgroundEmailQueue;
	}

	public async Task InvoiceInsertOnCreated(InvoiceUpsertProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@stripe_invoice_id", model.stripe_invoice_id);
		parameters.Add("@entity_id", model.entity_id);
		parameters.Add("@entity_type_id", model.entity_type_id);
		parameters.Add("@entity_type", model.entity_type);
		parameters.Add("@invoice_type", model.invoice_type);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@status", model.status);
		parameters.Add("@currency", model.currency);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@metadata_json", model.metadata_json);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_invoice_insert_on_created]", parameters, null, null, commandType);
	}

	public async Task InvoiceUpdateOnFinalized(InvoiceUpsertProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@stripe_invoice_id", model.stripe_invoice_id);
		parameters.Add("@status", model.status);
		parameters.Add("@amount_subtotal", model.amount_subtotal);
		parameters.Add("@amount_vat", model.amount_vat);
		parameters.Add("@amount_discount", model.amount_discount);
		parameters.Add("@amount_total", model.amount_total);
		parameters.Add("@invoice_url", model.invoice_url);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@stripe_invoice_number", model.stripe_invoice_number);
		parameters.Add("@stripe_receipt_number", model.stripe_receipt_number);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_invoice_update_on_finalized]", parameters, null, null, commandType);
	}

	public async Task InvoiceUpdateOnPaid(InvoiceUpsertProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DataTable dt = new DataTable();
		dt.Columns.Add("entity_type", typeof(string));
		dt.Columns.Add("entity_id", typeof(Guid));
		dt.Columns.Add("description", typeof(string));
		dt.Columns.Add("quantity", typeof(int));
		dt.Columns.Add("unit_price", typeof(decimal));
		dt.Columns.Add("total_price", typeof(decimal));
		List<InvoiceItemCreateDto>? items = model.Items;
		if (items != null && items.Count > 0)
		{
			foreach (InvoiceItemCreateDto item in model.Items)
			{
				dt.Rows.Add(item.entity_type, item.entity_id, item.description, item.quantity, item.unit_price, item.total_price);
			}
		}
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@stripe_invoice_id", model.stripe_invoice_id);
		parameters.Add("@entity_id", model.entity_id);
		parameters.Add("@entity_type_id", model.entity_type_id);
		parameters.Add("@entity_type", model.entity_type);
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@status", model.status);
		parameters.Add("@amount_subtotal", model.amount_subtotal);
		parameters.Add("@amount_vat", model.amount_vat);
		parameters.Add("@discount_amount", model.amount_discount);
		parameters.Add("@amount_total", model.amount_total);
		parameters.Add("@invoice_url", model.invoice_url);
		parameters.Add("@stripe_payment_intent_id", model.stripe_payment_intent_id);
		parameters.Add("@stripe_charge_id", model.stripe_charge_id);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@stripe_invoice_number", model.stripe_invoice_number);
		parameters.Add("@stripe_receipt_number", model.stripe_receipt_number);
		parameters.Add("@billing_period_start", model.billing_period_start);
		parameters.Add("@billing_period_end", model.billing_period_end);
		parameters.Add("@invoice_type", model.invoice_type);
		parameters.Add("@InvoiceItems", dt.AsTableValuedParameter("dbo.InvoiceItemTvp"));
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_invoice_update_on_paid]", parameters, null, null, commandType);
		InvEventEmailNotifyDto inv_event_notify = new InvEventEmailNotifyDto
		{
			stripe_invoice_id = model.stripe_invoice_id,
			user_id = model.user_id
		};
		await _backgroundEmailQueue.QueueInvoicePaidSuccessfullEmail(inv_event_notify);
	}

	public async Task InvoiceUpdateOnStatusChanged(string stripe_invoice_id, string status)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_invoice_id, status };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_invoice_update_on_status_changed]", param, null, null, commandType);
	}

	public async Task Invoice_event_process_updateAsync(InvoiceEventProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DataTable dt = new DataTable();
		dt.Columns.Add("entity_type", typeof(string));
		dt.Columns.Add("entity_id", typeof(Guid));
		dt.Columns.Add("description", typeof(string));
		dt.Columns.Add("quantity", typeof(int));
		dt.Columns.Add("unit_price", typeof(decimal));
		dt.Columns.Add("total_price", typeof(decimal));
		foreach (InvoiceItemCreateDto item in model.Items)
		{
			dt.Rows.Add(item.entity_type, item.entity_id, item.description, item.quantity, item.unit_price, item.total_price);
		}
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@stripe_invoice_id", model.stripe_invoice_id);
		parameters.Add("@stripe_payment_intent_id", model.stripe_payment_intent_id);
		parameters.Add("@status", model.status);
		parameters.Add("@invoice_type", model.invoice_type);
		parameters.Add("@subtotal", model.subtotal);
		parameters.Add("@currency", model.currency);
		parameters.Add("@tax_amount", model.tax_amount);
		parameters.Add("@discount_amount", model.discount_amount);
		parameters.Add("@total_amount", model.total_amount);
		parameters.Add("@billing_email", model.billing_email);
		parameters.Add("@billing_period_start", model.billing_period_start);
		parameters.Add("@billing_period_end", model.billing_period_end);
		parameters.Add("@issued_at", model.issued_at);
		parameters.Add("@paid_at", model.paid_at);
		parameters.Add("@due_at", model.due_at);
		parameters.Add("@metadata_json", model.metadata_json);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@event_type", model.event_type);
		parameters.Add("@invoice_url", model.invoice_url);
		parameters.Add("@entity_type_id", model.entity_type_id);
		parameters.Add("@entity_id", model.entity_id);
		parameters.Add("@InvoiceItems", dt.AsTableValuedParameter("dbo.InvoiceItemTvp"));
		parameters.Add("@actor", model.actor);
		parameters.Add("@source", model.source);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("dbo.usp_invoice_event_process_update", parameters, null, null, commandType);
	}

	public async Task<IEnumerable<Invoices>> GetByUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<Invoices>("usp_invoices_get_by_user", param, null, null, commandType);
	}

	public async Task<Invoices?> GetByIdAsync(Guid invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = invoice_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Invoices>("usp_invoices_get_by_id", param, null, null, commandType);
	}

	public async Task<Invoices?> GetByStripeInvoiceIdAsync(string stripe_invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_invoice_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Invoices>("usp_invoices_get_by_stripe_invoice_id", param, null, null, commandType);
	}

	public async Task MarkPaidAsync(string stripe_invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_invoice_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_invoices_mark_paid", param, null, null, commandType);
	}

	public async Task MarkFailedAsync(string stripe_invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_invoice_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_invoices_mark_failed", param, null, null, commandType);
	}

	public async Task<InvoiceListResponse> GetInvoiceListAsync(InvoiceListRequest invreq)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { invreq.user_id, invreq.page_number, invreq.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader InvoiceList = await connection.QueryMultipleAsync("[dbo].[usp_invoices_list]", param, null, null, commandType);
		InvoiceListResponse InvoiceListRes = new InvoiceListResponse();
		InvoiceListRes.items = InvoiceList.Read<InvoiceListItem>().ToList();
		InvoiceListRes.total_records = InvoiceList.ReadSingle<int>();
		return InvoiceListRes;
	}

	public async Task<BillingNotifyEmailDataModel> InvoicePaidNotifyEmailDetails(InvEventEmailNotifyDto InvPaidNotify)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { InvPaidNotify.stripe_invoice_id, InvPaidNotify.user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<BillingNotifyEmailDataModel>("[dbo].[usp_subscription_credit_purchase_notify_email]", param, null, null, commandType);
	}
}

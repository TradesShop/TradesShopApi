using Dapper;
using Stripe;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class InvoicesTshRepository: IInvoicesTshRepository
    {
        private readonly DapperContext _context;
        private readonly IIdentityService _identityService;

        public InvoicesTshRepository(DapperContext context, IIdentityService identityService)
        {
            _context = context;
            _identityService = identityService;
        }
        public async Task Invoice_event_process_updateAsync(InvoiceEventProcessDto model)
        {
            using var conn = _context.CreateOpenConnection();
            var dt = new DataTable();
            dt.Columns.Add("entity_type", typeof(string));
            dt.Columns.Add("entity_id", typeof(Guid));
            dt.Columns.Add("description", typeof(string));
            dt.Columns.Add("quantity", typeof(int));
            dt.Columns.Add("unit_price", typeof(decimal));
            dt.Columns.Add("total_price", typeof(decimal));

            foreach (var item in model.Items)
            {
                dt.Rows.Add(
                    item.entity_type,
                    item.entity_id,
                    item.description,
                    item.quantity,
                    item.unit_price,
                    item.total_price
                );
            }

            var parameters = new DynamicParameters();           
            parameters.Add("@user_id", model.user_id);
            parameters.Add("@plan_price_id", model.plan_price_id);
            parameters.Add("@stripe_invoice_id", model.stripe_invoice_id);
            parameters.Add("@stripe_payment_intent_id", model.stripe_payment_intent_id);
            parameters.Add("@status", model.status);
            parameters.Add("@invoice_type", model.currency);
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
            parameters.Add("@InvoiceItems", dt.AsTableValuedParameter("dbo.InvoiceItemTvp"));
            parameters.Add("@actor", model.actor);
            parameters.Add("@source", model.source);
            
            await conn.ExecuteAsync(
               "dbo.usp_invoice_event_process_update",
               parameters,
               commandType: CommandType.StoredProcedure
           );
        }



        public async Task<IEnumerable<Invoices>> GetByUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<Invoices>(
                "usp_invoices_get_by_user",
                new { user_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Invoices?> GetByIdAsync(Guid invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<Invoices>(
                "usp_invoices_get_by_id",
                new { id = invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Invoices?> GetByStripeInvoiceIdAsync(string stripe_invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<Invoices>(
                "usp_invoices_get_by_stripe_invoice_id",
                new { stripe_invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }

        //public async Task<Invoices> InsertInvoiceAsync(Invoices model)
        //{
        //    using var conn = _context.CreateOpenConnection();

        //    var anyivoice= await conn.QueryFirstOrDefaultAsync<Invoices>(
        //        "usp_invoices_create",
        //        new
        //        {
        //            model.id,
        //            model.user_id,
        //            model.invoice_number,
        //            model.type,
        //            model.status,
        //            model.currency,
        //            model.subtotal,
        //            model.tax_amount,
        //            model.discount_amount,
        //            model.total_amount,
        //            model.stripe_invoice_id,
        //            model.stripe_payment_intent_id,
        //            model.stripe_customer_id,
        //            model.billing_email,
        //            model.issued_at,
        //            model.paid_at,
        //            model.due_at,
        //            model.created_at,
        //            model.updated_at
        //        },
        //        commandType: CommandType.StoredProcedure
        //    );
        //    return anyivoice;
        //}

        public async Task UpdateAsync(Invoices model)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_invoices_update",
                new
                {
                    model.id,
                    model.user_id,
                    model.invoice_number,
                    model.type,
                    model.status,
                    model.currency,
                    model.subtotal,
                    model.tax_amount,
                    model.discount_amount,
                    model.total_amount,
                    model.stripe_invoice_id,
                    model.stripe_payment_intent_id,
                    model.stripe_customer_id,
                    model.billing_email,
                    model.issued_at,
                    model.paid_at,
                    model.due_at,
                    model.updated_at
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkPaidAsync(string stripe_invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_invoices_mark_paid",
                new { stripe_invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkFailedAsync(string stripe_invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_invoices_mark_failed",
                new { stripe_invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<PagedResultDto<Invoices>> GetPagedAsync(InvoiceQueryDto query)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QueryMultipleAsync(
                "usp_invoices_get_paged",
                new
                {
                    page = query.Page,
                    page_size = query.PageSize,
                    status = query.Status,
                    user_id = query.UserId,
                    invoice_number = query.InvoiceNumber,
                    from_date = query.FromDate,
                    to_date = query.ToDate,
                    sort_by = query.SortBy,
                    sort_direction = query.SortDirection
                },
                commandType: CommandType.StoredProcedure
            );

            var items = await result.ReadAsync<Invoices>();
            var total = await result.ReadFirstAsync<int>();

            return new PagedResultDto<Invoices>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = total,
                Items = items
            };
        }
    }
}

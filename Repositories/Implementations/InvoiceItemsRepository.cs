using System.Data;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class InvoiceItemsRepository : IInvoiceItemsRepository
    {
        private readonly DapperContext _context;

        public InvoiceItemsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvoiceItems>> GetByInvoiceIdAsync(Guid invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<InvoiceItems>(
                "usp_invoice_items_get_by_invoice_id",
                new { invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }
        //public async Task CreateManyAsync(IEnumerable<InvoiceItems> items)
        //{
        //    using var conn = _context.CreateOpenConnection();
        //    conn.Open(); // Required for TVP

        //    using var tx = conn.BeginTransaction();

        //    try
        //    {
        //        var table = new DataTable();
        //        table.Columns.Add("id", typeof(Guid));
        //        table.Columns.Add("invoice_id", typeof(Guid));
        //        table.Columns.Add("reference_type", typeof(string));
        //        table.Columns.Add("reference_id", typeof(Guid));
        //        table.Columns.Add("description", typeof(string));
        //        table.Columns.Add("quantity", typeof(int));
        //        table.Columns.Add("unit_price", typeof(decimal));
        //        table.Columns.Add("total_price", typeof(decimal));
        //        table.Columns.Add("metadata", typeof(string));
        //        table.Columns.Add("created_at", typeof(DateTime));

        //        foreach (var item in items)
        //        {
        //            table.Rows.Add(
        //                item.id,
        //                item.invoice_id,
        //                item.reference_type,
        //                item.reference_id,
        //                item.description,
        //                item.quantity,
        //                item.unit_price,
        //                item.total_price,
        //                item.metadata,
        //                item.created_at
        //            );
        //        }

        //        await conn.ExecuteAsync(
        //            "usp_invoice_items_create_many",
        //            new { Items = table.AsTableValuedParameter("InvoiceItemsType") },
        //            transaction: tx,
        //            commandType: CommandType.StoredProcedure
        //        );

        //        tx.Commit();
        //    }
        //    catch
        //    {
        //        tx.Rollback();
        //        throw;
        //    }
        //}

        //public async Task CreateManyAsync(IEnumerable<InvoiceItems> items)
        //{
        //    using var conn = _context.CreateOpenConnection();
        //    await conn.OpenAsync(); // <-- REQUIRED
        //    using var tx = conn.BeginTransaction();

        //    foreach (var item in items)
        //    {
        //        await conn.ExecuteAsync(
        //            "usp_invoice_items_create",
        //            new
        //            {
        //                item.id,
        //                item.invoice_id,
        //                item.reference_type,
        //                item.reference_id,
        //                item.description,
        //                item.quantity,
        //                item.unit_price,
        //                item.total_price,
        //                item.metadata,
        //                item.created_at
        //            },
        //            transaction: tx,
        //            commandType: CommandType.StoredProcedure
        //        );
        //    }

        //    tx.Commit();
        //}
    }
}

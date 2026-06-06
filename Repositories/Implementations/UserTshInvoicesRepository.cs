using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class UserTshInvoicesRepository : IUserTshInvoicesRepository
    {
        private readonly DapperContext _context;

        public UserTshInvoicesRepository(DapperContext context)
        {
            _context = context;
        }

        public Task<UserInvoices?> GetByStripeInvoiceIdAsync(string stripe_invoiceid)
        {
            using var conn = _context.CreateOpenConnection();
            return conn.QueryFirstOrDefaultAsync<UserInvoices>(
                "dbo.userinvoices_get_by_stripe_invoiceid",
                new { stripe_invoiceid },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IReadOnlyList<UserInvoices>> GetByUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<UserInvoices>(
                "dbo.userinvoices_get_by_user",
                new { user_id },
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public Task InsertAsync(UserInvoices entity)
        {
            using var conn = _context.CreateOpenConnection();
            return conn.ExecuteAsync(
                "dbo.userinvoices_create",
                new
                {
                    entity.id,
                    entity.user_id,
                    entity.stripe_invoiceid,
                    entity.stripe_paymentintentid,
                    entity.amount,
                    entity.currency,
                    entity.status,
                    entity.invoice_date,
                    entity.due_date,
                    entity.paid_at,
                    entity.created_at,
                    entity.updated_at
                },
                commandType: CommandType.StoredProcedure);
        }

        public Task UpdateAsync(UserInvoices entity)
        {
            using var conn = _context.CreateOpenConnection();
            return conn.ExecuteAsync(
                "dbo.userinvoices_update",
                new
                {
                    entity.id,
                    entity.stripe_paymentintentid,
                    entity.amount,
                    entity.currency,
                    entity.status,
                    entity.invoice_date,
                    entity.due_date,
                    entity.paid_at,
                    entity.updated_at
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}

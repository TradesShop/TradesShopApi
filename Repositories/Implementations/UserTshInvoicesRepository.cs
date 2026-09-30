using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class UserTshInvoicesRepository : IUserTshInvoicesRepository
{
	private readonly DapperContext _context;

	public UserTshInvoicesRepository(DapperContext context)
	{
		_context = context;
	}

	public Task<UserInvoices?> GetByStripeInvoiceIdAsync(string stripe_invoiceid)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_invoiceid };
		CommandType? commandType = CommandType.StoredProcedure;
		return conn.QueryFirstOrDefaultAsync<UserInvoices>("dbo.userinvoices_get_by_stripe_invoiceid", param, null, null, commandType);
	}

	public async Task<IReadOnlyList<UserInvoices>> GetByUserAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return (await conn.QueryAsync<UserInvoices>("dbo.userinvoices_get_by_user", param, null, null, commandType)).AsList();
	}

	public Task InsertAsync(UserInvoices entity)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			entity.id, entity.user_id, entity.stripe_invoiceid, entity.stripe_paymentintentid, entity.amount, entity.currency, entity.status, entity.invoice_date, entity.due_date, entity.paid_at,
			entity.created_at, entity.updated_at
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return conn.ExecuteAsync("dbo.userinvoices_create", param, null, null, commandType);
	}

	public Task UpdateAsync(UserInvoices entity)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { entity.id, entity.stripe_paymentintentid, entity.amount, entity.currency, entity.status, entity.invoice_date, entity.due_date, entity.paid_at, entity.updated_at };
		CommandType? commandType = CommandType.StoredProcedure;
		return conn.ExecuteAsync("dbo.userinvoices_update", param, null, null, commandType);
	}
}

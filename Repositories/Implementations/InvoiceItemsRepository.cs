using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class InvoiceItemsRepository : IInvoiceItemsRepository
{
	private readonly DapperContext _context;

	public InvoiceItemsRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<InvoiceItems>> GetByInvoiceIdAsync(Guid invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { invoice_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<InvoiceItems>("usp_invoice_items_get_by_invoice_id", param, null, null, commandType);
	}
}

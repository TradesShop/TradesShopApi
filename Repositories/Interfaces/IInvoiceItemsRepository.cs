using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IInvoiceItemsRepository
{
	Task<IEnumerable<InvoiceItems>> GetByInvoiceIdAsync(Guid invoice_id);
}

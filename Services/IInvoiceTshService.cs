using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Stripe;

namespace TradePlatform.Api.Services
{
    public interface IInvoiceTshService
    {
        Task<IReadOnlyList<InvoiceDto>> GetInvoicesForUserAsync(Guid user_id);
    }
}

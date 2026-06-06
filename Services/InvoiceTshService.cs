using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services
{
    public class InvoiceService : IInvoiceTshService
    {
        private readonly IUserTshInvoicesRepository _repo;

        public InvoiceService(IUserTshInvoicesRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesForUserAsync(Guid user_id)
        {
            var list = await _repo.GetByUserAsync(user_id);

            return list.Select(x => new InvoiceDto
            {
                //id = x.id,
                //stripe_invoiceid = x.stripe_invoiceid,
                //amount = x.amount,
                //currency = x.currency,
                //status = x.status,
                //invoice_date = x.invoice_date,
                //paid_at = x.paid_at
            }).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments;

public class PaymentsService : IPaymentsService
{
	private readonly IPaymentsRepository _paymentsRepo;

	private readonly ITdsInvoiceRepository _invoicesRepo;

	public PaymentsService(IPaymentsRepository paymentsRepo, ITdsInvoiceRepository invoicesRepo)
	{
		_paymentsRepo = paymentsRepo;
		_invoicesRepo = invoicesRepo;
	}

	public async Task<PaymentsM?> GetPaymentAsync(Guid payment_id)
	{
		PaymentsM payment = await _paymentsRepo.GetByIdAsync(payment_id);
		if (payment == null)
		{
			return null;
		}
		return payment;
	}

	public async Task<IEnumerable<PaymentsM>> GetPaymentsByInvoiceAsync(Guid invoice_id)
	{
		return await _paymentsRepo.GetByInvoiceIdAsync(invoice_id);
	}

	public async Task<IEnumerable<PaymentsM>> GetPaymentsByUserAsync(Guid user_id)
	{
		IEnumerable<TradePlatform.Api.Models.Invoices> invoices = await _invoicesRepo.GetByUserAsync(user_id);
		List<PaymentsM> allPayments = new List<PaymentsM>();
		foreach (TradePlatform.Api.Models.Invoices invoice in invoices)
		{
			allPayments.AddRange(await _paymentsRepo.GetByInvoiceIdAsync(invoice.id));
		}
		return allPayments;
	}

	public async Task<PaymentsM> RecordPaymentAsync(Guid user_id, Guid invoice_id, string stripe_payment_intent_id, string? stripe_charge_id, decimal amount, string currency, string status)
	{
		DateTime now = DateTime.UtcNow;
		PaymentsM payment = new PaymentsM
		{
			id = Guid.NewGuid(),
			user_id = user_id,
			invoice_id = invoice_id,
			stripe_payment_intent_id = stripe_payment_intent_id,
			stripe_charge_id = stripe_charge_id,
			amount = amount,
			currency = currency,
			status = status,
			created_at = now
		};
		await _paymentsRepo.CreateAsync(payment);
		return payment;
	}
}

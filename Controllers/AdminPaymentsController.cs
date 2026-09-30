using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/admin/payments")]
public class AdminPaymentsController : ControllerBase
{
	private readonly IPaymentsService _paymentsService;

	private readonly IRefundServices _refundService;

	public AdminPaymentsController(IPaymentsService paymentsService, IRefundServices refundService)
	{
		_paymentsService = paymentsService;
		_refundService = refundService;
	}

	[HttpGet("{payment_id:guid}")]
	public async Task<IActionResult> GetPayment(Guid payment_id)
	{
		PaymentsM payment = await _paymentsService.GetPaymentAsync(payment_id);
		if (payment == null)
		{
			return NotFound();
		}
		return Ok(payment);
	}

	[HttpGet("invoice/{invoice_id:guid}")]
	public async Task<IActionResult> GetPaymentsByInvoice(Guid invoice_id)
	{
		return Ok(await _paymentsService.GetPaymentsByInvoiceAsync(invoice_id));
	}

	[HttpGet("user/{user_id:guid}")]
	public async Task<IActionResult> GetPaymentsByUser(Guid user_id)
	{
		return Ok(await _paymentsService.GetPaymentsByUserAsync(user_id));
	}
}

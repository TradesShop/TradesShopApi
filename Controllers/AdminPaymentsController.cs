using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Controllers
{
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

        // ------------------------------------------------------------
        // GET: /api/admin/payments/{payment_id}
        // ------------------------------------------------------------
        [HttpGet("{payment_id:guid}")]
        public async Task<IActionResult> GetPayment(Guid payment_id)
        {
            var payment = await _paymentsService.GetPaymentAsync(payment_id);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        // ------------------------------------------------------------
        // GET: /api/admin/payments/invoice/{invoice_id}
        // ------------------------------------------------------------
        [HttpGet("invoice/{invoice_id:guid}")]
        public async Task<IActionResult> GetPaymentsByInvoice(Guid invoice_id)
        {
            var payments = await _paymentsService.GetPaymentsByInvoiceAsync(invoice_id);
            return Ok(payments);
        }

        // ------------------------------------------------------------
        // GET: /api/admin/payments/user/{user_id}
        // ------------------------------------------------------------
        [HttpGet("user/{user_id:guid}")]
        public async Task<IActionResult> GetPaymentsByUser(Guid user_id)
        {
            var payments = await _paymentsService.GetPaymentsByUserAsync(user_id);
            return Ok(payments);
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequestDto dto)
        {
            var success = await _refundService.RefundPaymentAsync(dto);

            if (!success)
                return BadRequest(new { message = "Refund failed" });

            return Ok(new { message = "Refund processed successfully" });
        }
    }
}

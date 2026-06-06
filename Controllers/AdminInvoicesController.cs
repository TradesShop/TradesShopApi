using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Services.Invoices;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/admin/invoices")]
    public class AdminInvoicesController : ControllerBase
    {
        private readonly IInvoicesTshService _invoicesService;

        public AdminInvoicesController(IInvoicesTshService invoicesService)
        {
            _invoicesService = invoicesService;
        }

        // ------------------------------------------------------------
        // GET: /api/admin/invoices/{invoice_id}
        // ------------------------------------------------------------
        //[HttpGet("{invoice_id:guid}")]
        //public async Task<IActionResult> GetInvoice(Guid invoice_id)
        //{
        //    var invoice = await _invoicesService.GetInvoiceAsync(invoice_id);
        //    if (invoice == null)
        //        return NotFound();

        //    return Ok(invoice);
        //}

        // ------------------------------------------------------------
        // GET: /api/admin/invoices/user/{user_id}
        // ------------------------------------------------------------
        //[HttpGet("user/{user_id:guid}")]
        //public async Task<IActionResult> GetInvoicesByUser(Guid user_id)
        //{
        //    // You can add a service method later if needed
        //    var invoice = await _invoicesService.GetInvoiceAsync(user_id);
        //    return Ok(invoice);
        //}

        // ------------------------------------------------------------
        // POST: /api/admin/invoices
        // Create invoice manually (admin only)
        // ------------------------------------------------------------
        //[HttpPost]
        //public async Task<IActionResult> CreateInvoice([FromBody] InvoiceEventProcessDto dto)
        //{
        //    var invoice = await _invoicesService.CreateInvoiceAsync(dto);
        //    return Ok(invoice);
        //}

        // ------------------------------------------------------------
        // PUT: /api/admin/invoices/{invoice_id}/mark-paid
        // ------------------------------------------------------------
        [HttpPut("{invoice_id:int}/mark-paid")]
        public async Task<IActionResult> MarkInvoicePaid(Guid invoice_id)
        {
            await _invoicesService.MarkInvoicePaidAsync(invoice_id, DateTime.UtcNow);
            return Ok(new { message = "Invoice marked as paid" });
        }

        // ------------------------------------------------------------
        // PUT: /api/admin/invoices/{invoice_id}/mark-failed
        // ------------------------------------------------------------
        [HttpPut("{invoice_id:int}/mark-failed")]
        public async Task<IActionResult> MarkInvoiceFailed(Guid invoice_id)
        {
            await _invoicesService.MarkInvoiceFailedAsync(invoice_id);
            return Ok(new { message = "Invoice marked as failed" });
        }
        [HttpGet]
        public async Task<IActionResult> GetPagedInvoices([FromQuery] InvoiceQueryDto query)
        {
            var result = await _invoicesService.GetPagedAsync(query);
            return Ok(result);
        }
    }
}
/*
 ✔ 1. Get invoice by ID

GET /api/admin/invoices/{invoice_id}  
Returns full invoice + items.
✔ 2. Get invoices by user

GET /api/admin/invoices/user/{user_id}  
(You can expand this later.)
✔ 3. Create invoice manually

POST /api/admin/invoices  
Useful for admin‑generated invoices (manual charges, adjustments, etc.)
✔ 4. Mark invoice as paid

PUT /api/admin/invoices/{invoice_id}/mark-paid  
Used for manual reconciliation.
✔ 5. Mark invoice as failed

PUT /api/admin/invoices/{invoice_id}/mark-failed  
Used for admin overrides.
 */
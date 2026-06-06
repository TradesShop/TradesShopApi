using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceTshService _service;

        public InvoicesController(IInvoiceTshService service)
        {
            _service = service;
        }

        private Guid UserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetInvoicesForUserAsync(UserId));
        }
    }
}

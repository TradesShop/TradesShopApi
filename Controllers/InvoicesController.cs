using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
	private readonly ITdsInvoiceService _service;

	public InvoicesController(ITdsInvoiceService service)
	{
		_service = service;
	}
}

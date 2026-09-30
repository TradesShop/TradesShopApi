using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Telemetry;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TelemetryErrorsController : BaseController
{
	private readonly ITelemetryErrorsService _service;

	public TelemetryErrorsController(ITelemetryErrorsService service)
	{
		_service = service;
	}

	[HttpPost("record")]
	public async Task<IActionResult> insert([FromBody] telemetry_error error)
	{
		(Guid? userId, UserType? userType) tuple = TryGetIdentity();
		var (user_id, _) = tuple;
		_ = tuple.userType;
		error.user_id = user_id;
		await _service.telemetry_error_insert_async(error);
		return ApiOk(new
		{
			success = true
		});
	}
}

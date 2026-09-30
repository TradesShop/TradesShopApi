using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.MagicPayLoad;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.AESHelper;
using TradePlatform.Api.Services.Cookies;
using TradePlatform.Api.Services.Telemetry;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Controllers;

[Route("tds-c")]
[Route("mts-c")]
[ApiController]
public class ClickController : BaseController
{
	private readonly IAesCryptoService _aes;

	private readonly IJwtTokenService _jwt;

	private readonly IUsersService _uservice;

	private readonly MagicLoginSettings _settings;

	private readonly ITelemetryErrorsService _telemetryService;

	private readonly IAuthService _auth;

	private readonly ICookieService _cookieService;

	public ClickController(IAesCryptoService aes, IJwtTokenService jwt, IUsersService uservice, IOptions<MagicLoginSettings> settings, ITelemetryErrorsService telemetryService, IAuthService auth, ICookieService cookieService)
	{
		_aes = aes;
		_jwt = jwt;
		_uservice = uservice;
		_settings = settings.Value;
		_telemetryService = telemetryService;
		_auth = auth;
		_cookieService = cookieService;
	}

	[HttpGet("jobs")]
	public async Task<IActionResult> ClickRedirect([FromQuery] string lk, [FromQuery] string redirect = null)
	{
		if (string.IsNullOrWhiteSpace(lk))
		{
			return BadRequest("Missing link token");
		}
		string encrypted = Uri.UnescapeDataString(lk);
		string json;
		try
		{
			json = _aes.Decrypt(encrypted);
		}
		catch
		{
			return Unauthorized("Invalid link");
		}
		MagicPayload payload;
		try
		{
			payload = JsonConvert.DeserializeObject<MagicPayload>(json);
		}
		catch
		{
			return Unauthorized("Invalid payload");
		}
		if (payload.expires_at < DateTime.UtcNow)
		{
			return Unauthorized("Link expired");
		}
		string target = (string.IsNullOrWhiteSpace(redirect) ? "/my-account" : redirect);
		if (string.IsNullOrWhiteSpace(target))
		{
			target = "/my-account";
		}
		User anyuser = await _uservice.GetUserByIdAsync(payload.user_id);
		user_event anyevent = new user_event
		{
			user_id = payload.user_id,
			entity_type_id = (payload.entity_type_id ?? EntityType.JobPosts.Id),
			entity_id = (payload.entity_id ?? payload.job_id),
			event_type_id = EventType.MagicUrlClick.Id,
			event_action_id = 110,
			source = "magic_url_click",
			payload = System.Text.Json.JsonSerializer.Serialize(payload)
		};
		await _uservice.user_event_insert_async(anyevent);
		RegisterResponse authResult = await _auth.CreateAuthTokensAsync(anyuser);
		_cookieService.ClearAuthCookies(Response);
		_cookieService.SetAuthCookies(Response, authResult.token, authResult.refresh_token);
		string redirectUrl = _settings.FrontendBaseUrl + target;
		return Redirect(redirectUrl);
	}

	[HttpGet("billing")]
	public async Task<IActionResult> RedirectBilling([FromQuery] string lk, [FromQuery] string redirect = null)
	{
		if (string.IsNullOrWhiteSpace(lk))
		{
			return BadRequest("Missing link token");
		}
		string encrypted = Uri.UnescapeDataString(lk);
		string json;
		try
		{
			json = _aes.Decrypt(encrypted);
		}
		catch
		{
			return Unauthorized("Invalid link");
		}
		MagicBillingload payload;
		try
		{
			payload = JsonConvert.DeserializeObject<MagicBillingload>(json);
		}
		catch
		{
			return Unauthorized("Invalid payload");
		}
		if (payload.expires_at < DateTime.UtcNow)
		{
			return Unauthorized("Link expired");
		}
		string target = (string.IsNullOrWhiteSpace(redirect) ? "/my-account" : redirect);
		if (string.IsNullOrWhiteSpace(target))
		{
			target = "/my-account";
		}
		User anyuser = await _uservice.GetUserByIdAsync(payload.user_id);
		user_event anyevent = new user_event
		{
			user_id = payload.user_id,
			entity_type_id = (payload.entity_type_id ?? EntityType.Subscription.Id),
			entity_id = payload.entity_id,
			event_type_id = EventType.MagicUrlClick.Id,
			event_action_id = 110,
			source = "magic_url_click",
			payload = System.Text.Json.JsonSerializer.Serialize(payload)
		};
		await _uservice.user_event_insert_async(anyevent);
		RegisterResponse authResult = await _auth.CreateAuthTokensAsync(anyuser);
		_cookieService.ClearAuthCookies(Response);
		_cookieService.SetAuthCookies(Response, authResult.token, authResult.refresh_token);
		string redirectUrl = _settings.FrontendBaseUrl + target;
		return Redirect(redirectUrl);
	}
}

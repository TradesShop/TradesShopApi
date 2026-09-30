using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
	protected IIdentityService? _identity => HttpContext?.RequestServices?.GetService<IIdentityService>();

	protected (Guid? userId, UserType? userType) TryGetIdentity()
	{
		return _identity?.TryGetIdentity() ?? (null, null);
	}

	protected (Guid userId, UserType userType) GetIdentity()
	{
		if (_identity == null)
		{
			throw new UnauthorizedAccessException("Identity service is unavailable or HttpContext is null.");
		}
		return _identity.GetIdentity();
	}

	protected Guid GetUserId()
	{
		return GetIdentity().userId;
	}

	protected UserType GetUserType()
	{
		return GetIdentity().userType;
	}

	protected IActionResult ApiOk(object? data = null, string message = "")
	{
		return Ok(new ApiResponse<object>
		{
			success = true,
			message = message,
			data = data
		});
	}

	protected IActionResult ApiError(object? data = null, string message = "", int status = 400)
	{
		return StatusCode(status, new ApiResponse<object>
		{
			success = false,
			message = message,
			data = data
		});
	}

	protected Guid ResolveEffectiveUser(Guid callerId, UserType callerType, Guid? targetUserId)
	{
		if (callerType != UserType.admin || !targetUserId.HasValue)
		{
			return callerId;
		}
		return targetUserId.Value;
	}

	protected string? GetIpAddress()
	{
		HttpContext httpContext = HttpContext;
		string ip = ((httpContext != null) ? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() : null);
		if (!string.IsNullOrWhiteSpace(ip))
		{
			return ip;
		}
		return httpContext?.Connection.RemoteIpAddress?.ToString();
	}

	protected string? GetUserAgent()
	{
		return HttpContext?.Request.Headers["User-Agent"].ToString();
	}

	protected string GenerateSlug(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}
		string slug = name.ToLowerInvariant();
		slug = Regex.Replace(slug, "[^a-z0-9]+", "-");
		slug = slug.Trim('-');
		return Regex.Replace(slug, "-+", "-");
	}
}

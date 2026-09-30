using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services;

public class IdentityService : IIdentityService
{
	private readonly IHttpContextAccessor _http;

	public IdentityService(IHttpContextAccessor http)
	{
		_http = http;
	}

	public (Guid? userId, UserType? userType) TryGetIdentity()
	{
		ClaimsPrincipal user = _http.HttpContext?.User;
		if (user == null || user.Identity?.IsAuthenticated != true)
		{
			return (userId: null, userType: null);
		}
		string userIdClaim = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") ?? user.FindFirstValue("sub") ?? user.FindFirstValue("sub");
		string roleClaim = user.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? user.FindFirstValue("role");
		Guid? userId = (Guid.TryParse(userIdClaim, out var parsedId) ? new Guid?(parsedId) : ((Guid?)null));
		UserType? userType = (Enum.TryParse<UserType>(roleClaim, ignoreCase: true, out var parsedRole) ? new UserType?(parsedRole) : ((UserType?)null));
		return (userId: userId, userType: userType);
	}

	public (Guid userId, UserType userType) GetIdentity()
	{
		var (userId, userType) = TryGetIdentity();
		if (!userId.HasValue)
		{
			throw new UnauthorizedAccessException("User is not authenticated or missing a valid User ID.");
		}
		if (!userType.HasValue)
		{
			throw new UnauthorizedAccessException("User role/type is missing or invalid in the access token.");
		}
		return (userId: userId.Value, userType: userType.Value);
	}

	public string GetUserEmail()
	{
		ClaimsPrincipal user = _http.HttpContext?.User;
		if (user == null || user.Identity?.IsAuthenticated != true)
		{
			throw new UnauthorizedAccessException("User is not authenticated.");
		}
		string email = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") ?? user.FindFirstValue("email") ?? user.FindFirstValue("email");
		if (string.IsNullOrWhiteSpace(email))
		{
			throw new UnauthorizedAccessException("Missing email in token.");
		}
		return email;
	}

	public Guid GetUserId()
	{
		return GetIdentity().userId;
	}

	public UserType GetUserType()
	{
		return GetIdentity().userType;
	}

	public Guid GetCurrentUserId()
	{
		return GetUserId();
	}

	public string GetIpAddress()
	{
		HttpContext httpContext = _http.HttpContext ?? throw new Exception("No HttpContext available");
		string ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(ip))
		{
			return ip;
		}
		return httpContext.Connection.RemoteIpAddress?.ToString();
	}

	public string GetUserAgent()
	{
		HttpContext httpContext = _http.HttpContext ?? throw new Exception("No HttpContext available");
		return httpContext.Request.Headers["User-Agent"].ToString();
	}
}

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace TradePlatform.Api.Services.Cookies;

public class CookieService : ICookieService
{
	private readonly IWebHostEnvironment _env;

	public CookieService(IWebHostEnvironment env)
	{
		_env = env;
	}

	private CookieOptions GetCookieOptions()
	{
		bool isDev = _env.IsDevelopment();
		return new CookieOptions
		{
			HttpOnly = true,
			Secure = !isDev,
			SameSite = SameSiteMode.Lax,
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddDays(90.0),
			Domain = (isDev ? null : ".tradesshop.co.uk"),
			IsEssential = true
		};
	}

	public void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
	{
		CookieOptions options = GetCookieOptions();
		string cleanAccessToken = accessToken?.Trim().Trim('"');
		string cleanRefreshToken = refreshToken?.Trim().Trim('"');
		if (!string.IsNullOrEmpty(cleanAccessToken))
		{
			response.Cookies.Append("auth_token", cleanAccessToken, options);
		}
		if (!string.IsNullOrEmpty(cleanRefreshToken))
		{
			response.Cookies.Append("refresh_token", cleanRefreshToken, options);
		}
	}

	public void ClearAuthCookies(HttpResponse response)
	{
		CookieOptions options = GetCookieOptions();
		response.Cookies.Delete("auth_token", options);
		response.Cookies.Delete("refresh_token", options);
		CookieOptions legacyOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = options.Secure,
			SameSite = options.SameSite,
			Path = "/",
			Domain = options.Domain,
			Expires = DateTimeOffset.UtcNow.AddDays(-10.0)
		};
		response.Cookies.Append("auth_token", "", legacyOptions);
		response.Cookies.Append("refresh_token", "", legacyOptions);
	}
}

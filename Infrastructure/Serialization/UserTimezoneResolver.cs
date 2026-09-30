using System;
using Microsoft.AspNetCore.Http;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class UserTimezoneResolver : IUserTimezoneResolver
{
	private readonly IHttpContextAccessor _http;

	public UserTimezoneResolver(IHttpContextAccessor http)
	{
		_http = http;
	}

	public TimeZoneInfo GetUserTimezone()
	{
		return (_http.HttpContext?.User?.FindFirst("country")?.Value ?? "UK") switch
		{
			"UK" => TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"), 
			"IN" => TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"), 
			"US" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), 
			_ => TimeZoneInfo.Utc, 
		};
	}
}

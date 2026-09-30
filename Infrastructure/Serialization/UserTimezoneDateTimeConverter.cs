using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class UserTimezoneDateTimeConverter : JsonConverter<DateTime>
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserTimezoneDateTimeConverter(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetDateTime();
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		TimeZoneInfo timezone = (_httpContextAccessor.HttpContext?.RequestServices.GetService<IUserTimezoneResolver>())?.GetUserTimezone() ?? TimeZoneInfo.Utc;
		DateTime utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
		DateTime converted = TimeZoneInfo.ConvertTimeFromUtc(utc, timezone);
		writer.WriteStringValue(converted);
	}
}

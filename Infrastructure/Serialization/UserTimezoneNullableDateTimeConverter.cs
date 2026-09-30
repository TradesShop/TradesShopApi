using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class UserTimezoneNullableDateTimeConverter : JsonConverter<DateTime?>
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserTimezoneNullableDateTimeConverter(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}
		return reader.GetDateTime();
	}

	public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
	{
		if (!value.HasValue)
		{
			writer.WriteNullValue();
			return;
		}
		TimeZoneInfo timezone = (_httpContextAccessor.HttpContext?.RequestServices.GetService<IUserTimezoneResolver>())?.GetUserTimezone() ?? TimeZoneInfo.Utc;
		DateTime utc = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
		DateTime converted = TimeZoneInfo.ConvertTimeFromUtc(utc, timezone);
		writer.WriteStringValue(converted);
	}
}

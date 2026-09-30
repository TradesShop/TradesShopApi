using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class UserTimezoneModelConverterFactory : JsonConverterFactory
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserTimezoneModelConverterFactory(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public override bool CanConvert(Type typeToConvert)
	{
		if (!(typeToConvert == typeof(DateTime)))
		{
			return typeToConvert == typeof(DateTime?);
		}
		return true;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert == typeof(DateTime))
		{
			return new UserTimezoneDateTimeConverter(_httpContextAccessor);
		}
		if (typeToConvert == typeof(DateTime?))
		{
			return new UserTimezoneNullableDateTimeConverter(_httpContextAccessor);
		}
		throw new NotSupportedException($"Type {typeToConvert} is not supported by {"UserTimezoneModelConverterFactory"}.");
	}
}

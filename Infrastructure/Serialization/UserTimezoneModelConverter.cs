using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class UserTimezoneModelConverter<T> : JsonConverter<T> where T : class, new()
{
	private readonly IUserTimezoneResolver _resolver;

	public UserTimezoneModelConverter(IUserTimezoneResolver resolver)
	{
		_resolver = resolver;
	}

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return JsonSerializer.Deserialize<T>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		TimeZoneInfo tz = _resolver.GetUserTimezone();
		Type type = typeof(T);
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo prop in properties)
		{
			if (prop.PropertyType == typeof(DateTime) && !prop.IsDefined(typeof(NoDateTimeConversionAttribute), inherit: true))
			{
				DateTime dt = (DateTime)prop.GetValue(value);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc), tz);
				prop.SetValue(value, converted);
			}
		}
		JsonSerializer.Serialize(writer, value, options);
	}
}

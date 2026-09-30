using System;
using System.Data;
using System.Text.Json;
using Dapper;

namespace TradePlatform.Api.Helpers;

public class JsonObjectTypeHandler<T> : SqlMapper.TypeHandler<T?> where T : class
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public override void SetValue(IDbDataParameter parameter, T? value)
	{
		parameter.Value = ((value == null) ? ((IConvertible)DBNull.Value) : ((IConvertible)JsonSerializer.Serialize(value, Options)));
	}

	public override T? Parse(object value)
	{
		if (value == null || value is DBNull)
		{
			return null;
		}
		string json = value.ToString()?.Trim();
		if (string.IsNullOrWhiteSpace(json) || json.Equals("null", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		try
		{
			return JsonSerializer.Deserialize<T>(json, Options);
		}
		catch
		{
			return null;
		}
	}
}

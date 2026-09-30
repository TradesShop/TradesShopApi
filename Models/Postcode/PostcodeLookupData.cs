using System.Collections.Generic;

namespace TradePlatform.Api.Models.Postcode;

public sealed class PostcodeLookupData
{
	public string postcode { get; init; }

	public string country { get; init; }

	public string country_code { get; init; }

	public string? city { get; init; }

	public string? district { get; init; }

	public string? region { get; init; }

	public double? latitude { get; init; }

	public double? longitude { get; init; }

	public Dictionary<string, object?> meta { get; init; } = new Dictionary<string, object>();
}

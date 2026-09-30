using System;

namespace TradePlatform.Api.Models;

public class UserAddress
{
	public Guid? user_id { get; set; }

	public int? address_id { get; set; }

	public Guid? business_id { get; set; }

	public string? address_line1 { get; set; }

	public string? address_line2 { get; set; }

	public string? town { get; set; }

	public string? county { get; set; }

	public string postcode { get; set; }

	public string country_code { get; set; }

	public int? country_id { get; set; } = 1015;

	public string? country_iso_code { get; set; } = "GB";

	public bool? is_primary { get; set; }

	public string? address_type { get; set; }

	public int? address_type_id { get; set; }

	public int? service_radius_km { get; set; }

	public decimal? latitude { get; set; }

	public decimal? longitude { get; set; }

	public bool success { get; set; }

	public string? message { get; set; }
}

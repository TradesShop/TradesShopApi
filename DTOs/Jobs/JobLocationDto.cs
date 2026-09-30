namespace TradePlatform.Api.DTOs.Jobs;

public class JobLocationDto : JobLocationMeta
{
	public string? address_line1 { get; set; }

	public string? address_line2 { get; set; }

	public string? city { get; set; }

	public string postcode { get; set; }

	public string country_code { get; set; }

	public decimal? latitude { get; set; }

	public decimal? longitude { get; set; }

	public string workplace { get; set; }
}

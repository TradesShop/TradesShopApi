namespace TradePlatform.Api.DTOs.users;

public class TradesPeopleSearchDto
{
	public string? search { get; set; }

	public string sort_by { get; set; }

	public int page_number { get; set; }

	public int page_size { get; set; }
}

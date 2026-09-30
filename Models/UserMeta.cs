namespace TradePlatform.Api.Models;

public class UserMeta : User
{
	public string postcode { get; set; }

	public decimal gLng { get; set; }

	public decimal gLat { get; set; }
}

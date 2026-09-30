namespace TradePlatform.Api.Models.MagicPayLoad;

public class MagicLoginSettings
{
	public string Key { get; set; }

	public string IV { get; set; }

	public string? BaseUrl { get; set; }

	public string? BaseBillingUrl { get; set; }

	public string FrontendBaseUrl { get; set; }
}

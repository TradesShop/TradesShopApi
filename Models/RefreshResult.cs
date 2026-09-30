namespace TradePlatform.Api.Models;

public class RefreshResult
{
	public bool success { get; set; }

	public string token { get; set; } = string.Empty;

	public string refresh_token { get; set; } = string.Empty;

	public static RefreshResult Fail()
	{
		return new RefreshResult
		{
			success = false
		};
	}

	public static RefreshResult Ok(string accessToken, string refreshToken)
	{
		return new RefreshResult
		{
			success = true,
			token = accessToken,
			refresh_token = refreshToken
		};
	}
}

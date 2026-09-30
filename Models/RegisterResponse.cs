namespace TradePlatform.Api.Models;

public class RegisterResponse
{
	public string? token { get; set; }

	public string? refresh_token { get; set; }

	public string message { get; set; }

	public object? User { get; set; }
}

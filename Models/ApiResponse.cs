namespace TradePlatform.Api.Models;

public class ApiResponse<T>
{
	public bool success { get; set; }

	public string message { get; set; } = "";

	public T? data { get; set; }
}

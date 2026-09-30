namespace TradePlatform.Api.Models.Postcode;

public sealed class PostcodeLookupResponse
{
	public bool success { get; init; }

	public string? error { get; init; }

	public PostcodeLookupData? data { get; init; }

	public static PostcodeLookupResponse Ok(PostcodeLookupData data)
	{
		return new PostcodeLookupResponse
		{
			success = true,
			data = data,
			error = null
		};
	}

	public static PostcodeLookupResponse Fail(string message)
	{
		return new PostcodeLookupResponse
		{
			success = false,
			error = message,
			data = null
		};
	}
}

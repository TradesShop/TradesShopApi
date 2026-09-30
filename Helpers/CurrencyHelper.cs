public static class CurrencyHelper
{
	public static string FormatAmount(decimal? amount = 0.00m, string? currencyCode = "gbp")
	{
		decimal actualAmount = amount ?? 0.00m;
		string code = (string.IsNullOrWhiteSpace(currencyCode) ? "GBP" : currencyCode.Trim().ToUpperInvariant());
		string symbol = code switch
		{
			"USD" => "$", 
			"CAD" => "$", 
			"AUD" => "$", 
			"GBP" => "£", 
			"EUR" => "€", 
			"INR" => "₹", 
			"JPY" => "¥", 
			_ => "£", 
		};
		if (!(code == "JPY"))
		{
			return $"{symbol}{actualAmount:N2} {code}";
		}
		return $"{symbol}{actualAmount:N0} {code}";
	}

	public static string FormatAmount(long amountInCents, string currencyCode)
	{
		string code = (currencyCode ?? "USD").Trim().ToUpperInvariant();
		decimal amount = ((code == "JPY") ? ((decimal)amountInCents) : ((decimal)amountInCents / 100m));
		return FormatAmount(amount, code);
	}
}

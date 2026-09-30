namespace TradePlatform.Api.Services.Google_OCR;

public static class MrzChecksum
{
	private static readonly int[] Weights = new int[3] { 7, 3, 1 };

	public static bool Validate(string input)
	{
		int sum = 0;
		for (int i = 0; i < input.Length - 1; i++)
		{
			char c = input[i];
			int value = 0;
			if (char.IsDigit(c))
			{
				value = c - 48;
			}
			else if (char.IsLetter(c))
			{
				value = c - 65 + 10;
			}
			sum += value * Weights[i % 3];
		}
		int checkDigit = input[input.Length - 1] - 48;
		return sum % 10 == checkDigit;
	}
}

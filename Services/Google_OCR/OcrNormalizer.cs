using System.Text.RegularExpressions;

namespace TradePlatform.Api.Services.Google_OCR;

public class OcrNormalizer
{
	public static string Normalize(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		text = text.Replace("\r", "\n");
		text = text.Replace("MODLESEX", "MIDDLESEX").Replace("MODDLESEX", "MIDDLESEX");
		text = Regex.Replace(text, "[ ]{2,}", " ");
		text = Regex.Replace(text, "\\n{2,}", "\n");
		return text.Trim();
	}
}

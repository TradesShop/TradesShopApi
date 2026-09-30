using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Google_OCR;

public class UnifiedDocumentParserService_x
{
	private readonly MrzParserService _mrz;

	public UnifiedDocumentParserService_x(MrzParserService mrz)
	{
		_mrz = mrz;
	}

	public VerifiedDocument Parse(string documentType, string text)
	{
		return documentType switch
		{
			"passport" => ParsePassport(text), 
			"driving_licence" => ParseDrivingLicence(text), 
			"brp" => ParseBrp(text), 
			_ => new VerifiedDocument
			{
				is_valid = false
			}, 
		};
	}

	private VerifiedDocument ParsePassport(string ocr)
	{
		VerifiedDocument result = new VerifiedDocument
		{
			document_type = "passport",
			nationality = "GBR",
			visa_type = null,
			is_valid = false
		};
		ocr = OcrNormalizer.Normalize(ocr);
		List<string> lines = (from x in ocr.Split('\n')
			select x.Trim() into x
			where !string.IsNullOrWhiteSpace(x)
			select x).ToList();
		string mrz1 = lines.FirstOrDefault((string x) => x.StartsWith("P<"));
		string mrz2 = ((mrz1 != null) ? lines.SkipWhile((string x) => x != mrz1).Skip(1).FirstOrDefault() : null);
		if (!string.IsNullOrEmpty(mrz1) && !string.IsNullOrEmpty(mrz2))
		{
			ParseMrz(mrz1, mrz2, result);
		}
		else
		{
			ParseVisual(lines, result);
		}
		result.is_valid = !string.IsNullOrWhiteSpace(result.document_number);
		return result;
	}

	private static void ParseMrz(string l1, string l2, VerifiedDocument r)
	{
		l1 = l1.PadRight(44, '<');
		l2 = l2.PadRight(44, '<');
		string[] namePart = l1.Substring(5).Split("<<");
		r.surname = namePart[0].Replace("<", " ").Trim();
		r.given_names = ((namePart.Length > 1) ? namePart[1].Replace("<", " ").Trim() : null);
		r.document_number = l2.Substring(0, 9).Replace("<", "");
		r.nationality = l2.Substring(10, 3);
		r.date_of_birth = ParseMrzDate(l2.Substring(13, 6));
		r.expiry_date = ParseMrzDate(l2.Substring(21, 6));
	}

	private static void ParseVisual(List<string> lines, VerifiedDocument r)
	{
		int idx = lines.FindIndex((string x) => x == "GBR");
		if (idx >= 0)
		{
			r.surname = lines.ElementAtOrDefault(idx + 1);
			r.given_names = lines.ElementAtOrDefault(idx + 2);
			r.document_number = lines.ElementAtOrDefault(idx + 3);
		}
		string dob = lines.FirstOrDefault((string x) => Regex.IsMatch(x, "\\d{2}\\s*-\\s*[A-Z]{3}"));
		if (dob != null)
		{
			r.date_of_birth = dob;
		}
	}

	private static string ParseMrzDate(string v)
	{
		int yy = int.Parse(v.Substring(0, 2));
		int mm = int.Parse(v.Substring(2, 2));
		int dd = int.Parse(v.Substring(4, 2));
		int year = ((yy > DateTime.Now.Year % 100) ? (1900 + yy) : (2000 + yy));
		return $"{year:0000}-{mm:00}-{dd:00}";
	}

	private Dictionary<string, string> BuildFieldMap(List<string> lines)
	{
		Dictionary<string, string> map = new Dictionary<string, string>();
		string currentField = null;
		foreach (string line in lines)
		{
			Match m = Regex.Match(line, "^(1|2|3|4a|4b|4|5|8|9)\\b", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				currentField = m.Groups[1].Value.ToLower();
				map[currentField] = line.Substring(m.Length).Trim();
			}
			else if (currentField != null)
			{
				Dictionary<string, string> dictionary = map;
				string key = currentField;
				dictionary[key] = dictionary[key] + " " + line;
			}
		}
		return map;
	}

	private string ExtractIssueDate(string ocr)
	{
		ocr = ocr.Replace(".", " ");
		Match match = Regex.Match(ocr, "4a\\s*(\\d{2})\\s*(\\d{2})\\s*(\\d{4})", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return $"{match.Groups[3].Value}-{match.Groups[2].Value}-{match.Groups[1].Value}";
		}
		Match fallback = Regex.Match(ocr, "(\\d{2})\\s+(\\d{2})\\s+(\\d{4})\\s+4\\s*DVLA", RegexOptions.IgnoreCase);
		if (fallback.Success)
		{
			return $"{fallback.Groups[3].Value}-{fallback.Groups[2].Value}-{fallback.Groups[1].Value}";
		}
		return null;
	}

	private string GetFieldValue(Dictionary<string, string> map, string key)
	{
		if (!map.ContainsKey(key))
		{
			return null;
		}
		return map[key].Trim();
	}

	private string ParseDateFlexible(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return null;
		}
		Match m = Regex.Match(input, "(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})");
		if (!m.Success)
		{
			return null;
		}
		return $"{m.Groups[3].Value}-{m.Groups[2].Value}-{m.Groups[1].Value}";
	}

	private string ExtractExpiry(string ocr)
	{
		MatchCollection matches = Regex.Matches(ocr, "(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})");
		foreach (Match m in matches)
		{
			int year = int.Parse(m.Groups[3].Value);
			if (year >= DateTime.Now.Year)
			{
				return $"{m.Groups[3].Value}-{m.Groups[2].Value}-{m.Groups[1].Value}";
			}
		}
		return null;
	}

	private string ExtractLicenceNumber(string ocr)
	{
		Match m = Regex.Match(ocr.Replace(" ", ""), "[A-Z]{5}\\d{6}[A-Z0-9]{5}");
		if (!m.Success)
		{
			return null;
		}
		return m.Value;
	}

	private string ExtractAddress(string ocr)
	{
		if (string.IsNullOrWhiteSpace(ocr))
		{
			return null;
		}
		string cleaned = Regex.Replace(ocr, "^(1|2|3|4a|4b|4|5|8)\\s.*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		cleaned = cleaned.Replace("&", " ").Replace("UK DRIVING LICENCE", " ");
		cleaned = Regex.Replace(cleaned, "\\s{2,}", " ");
		List<string> lines = (from x in cleaned.Split('\n')
			select x.Trim() into x
			where !string.IsNullOrWhiteSpace(x)
			select x).ToList();
		List<string> addressParts = lines.Where((string x) => !Regex.IsMatch(x, "^[A-Z]{1,2}\\d[A-Z\\d]?\\s*\\d[A-Z]{2}$") && !Regex.IsMatch(x, "^\\d+$") && !Regex.IsMatch(x, "^(AM|A|B1|B|BE|C|D|E)(/|$)")).ToList();
		return string.Join(" ", addressParts).Trim();
	}

	private VerifiedDocument ParseDrivingLicence(string ocr)
	{
		VerifiedDocument result = new VerifiedDocument
		{
			document_type = "driving_licence",
			nationality = "GBR",
			visa_type = null,
			is_valid = false
		};
		if (string.IsNullOrWhiteSpace(ocr))
		{
			return result;
		}
		ocr = Normalize(ocr);
		List<string> lines = (from x in ocr.Split('\n')
			select x.Trim() into x
			where !string.IsNullOrWhiteSpace(x)
			select x).ToList();
		Dictionary<string, string> fieldMap = BuildFieldMap(lines);
		result.document_number = ExtractLicenceNumber(ocr);
		result.surname = GetFieldValue(fieldMap, "1");
		string given = GetFieldValue(fieldMap, "2");
		result.given_names = CleanName(given);
		string dobRaw = GetFieldValue(fieldMap, "3");
		result.date_of_birth = ParseDateFlexible(dobRaw);
		string issueRaw = GetFieldValue(fieldMap, "4a");
		result.issue_date = ExtractIssueDate(ocr);
		result.expiry_date = ExtractExpiry(ocr);
		result.address = ExtractAddress(ocr);
		result.is_valid = !string.IsNullOrWhiteSpace(result.document_number);
		return result;
	}

	private string Normalize(string ocr)
	{
		ocr = ocr.Replace("\r", "\n");
		ocr = ocr.Replace("MODLESEX", "MIDDLESEX").Replace("MODDLESEX", "MIDDLESEX");
		ocr = Regex.Replace(ocr, "[ ]{2,}", " ");
		ocr = Regex.Replace(ocr, "\\n{2,}", "\n");
		return ocr;
	}

	private string CleanName(string name)
	{
		return name.Replace(".", "").Replace(":", "").Trim();
	}

	private VerifiedDocument ParseBrp(string text)
	{
		string upper = text.ToUpperInvariant();
		Match brpMatch = Regex.Match(upper, "\\b[A-Z]{2}\\d{7}[A-Z]\\b");
		string brpNumber = (brpMatch.Success ? brpMatch.Value : null);
		Match dobMatch = Regex.Match(upper, "(DOB|DATE OF BIRTH)\\s*[:\\-]?\\s*(\\d{2}[\\/\\-]\\d{2}[\\/\\-]\\d{4})");
		string dob = (dobMatch.Success ? dobMatch.Groups[2].Value : null);
		Match expiryMatch = Regex.Match(upper, "(EXPIRY|VALID UNTIL|EXP)\\s*[:\\-]?\\s*(\\d{2}[\\/\\-]\\d{2}[\\/\\-]\\d{4})");
		string expiry = (expiryMatch.Success ? expiryMatch.Groups[2].Value : null);
		Match visaMatch = Regex.Match(upper, "(TYPE|REMARKS|CATEGORY)\\s*[:\\-]?\\s*([A-Z0-9 \\-/]+)");
		string visaType = (visaMatch.Success ? visaMatch.Groups[2].Value.Trim() : null);
		string surname = ExtractLabelValue(upper, "SURNAME|FAMILY NAME");
		string givenNames = ExtractLabelValue(upper, "GIVEN NAMES|FORENAMES|FIRST NAMES");
		string nationality = ExtractLabelValue(upper, "NATIONALITY");
		return new VerifiedDocument
		{
			document_number = brpNumber,
			surname = surname,
			given_names = givenNames,
			nationality = nationality,
			date_of_birth = dob,
			expiry_date = expiry,
			issue_date = null,
			address = null,
			visa_type = null,
			is_valid = (brpNumber != null && dob != null && expiry != null)
		};
	}

	private string ExtractLabelValue(string text, string labelPattern)
	{
		Match match = Regex.Match(text, "(" + labelPattern + ")\\s*[:\\-]?\\s*([A-Z0-9 ,\\-\\/]+)");
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[2].Value.Trim();
	}
}

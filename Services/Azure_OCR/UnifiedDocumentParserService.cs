using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Azure_OCR;

public class UnifiedDocumentParserService
{
	private readonly MrzParserService _mrz;

	public UnifiedDocumentParserService(MrzParserService mrz)
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

	private VerifiedDocument ParsePassport(string text)
	{
		List<string> mrz = _mrz.ExtractMRZ(text);
		ParsedMrz parsed = _mrz.Parse(mrz);
		return new VerifiedDocument
		{
			document_number = parsed.document_number,
			surname = parsed.surname,
			given_names = parsed.given_names,
			nationality = parsed.nationality,
			date_of_birth = parsed.date_of_birth,
			expiry_date = parsed.expiry_date,
			issue_date = null,
			address = null,
			visa_type = null,
			is_valid = parsed.valid
		};
	}

	private VerifiedDocument ParseDrivingLicence(string text)
	{
		string upper = text.ToUpperInvariant();
		List<string> lines = (from l in upper.Split('\n')
			select l.Trim()).ToList();
		string surname = null;
		Match f1 = Regex.Match(upper, "\\b1[\\. ]+\\s*([A-Z]+)\\b");
		if (f1.Success)
		{
			surname = f1.Groups[1].Value;
		}
		string givenNames = null;
		Match f2 = Regex.Match(upper, "\\b2[\\. ]+\\s*([A-Z ]+)");
		if (f2.Success)
		{
			givenNames = CleanName(f2.Groups[1].Value);
		}
		if (givenNames == null)
		{
			Match at = Regex.Match(upper, "@\\s*([A-Z ]+)");
			if (at.Success)
			{
				givenNames = CleanName(at.Groups[1].Value);
			}
		}
		if (givenNames == null)
		{
			MatchCollection caps = Regex.Matches(upper, "\\b[A-Z]{2,}\\b");
			if (caps.Count > 1)
			{
				givenNames = caps[1].Value;
			}
		}
		string dob = null;
		Match f3 = Regex.Match(upper, "\\b3[\\. ]+\\s*(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})");
		if (f3.Success)
		{
			dob = $"{f3.Groups[3].Value}-{f3.Groups[2].Value}-{f3.Groups[1].Value}";
		}
		string issue = null;
		Match f4a = Regex.Match(upper, "4A[\\. ]+\\s*(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})");
		if (f4a.Success)
		{
			issue = $"{f4a.Groups[3].Value}-{f4a.Groups[2].Value}-{f4a.Groups[1].Value}";
		}
		string expiry = null;
		Match f4b = Regex.Match(upper, "4B[\\. ]+\\s*(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})");
		if (f4b.Success)
		{
			expiry = $"{f4b.Groups[3].Value}-{f4b.Groups[2].Value}-{f4b.Groups[1].Value}";
		}
		string licenceNumber = null;
		Regex licenceRegex = new Regex("\\b[A-Z]{5}\\d{6}[A-Z]\\d{2}[A-Z]{2}\\b", RegexOptions.IgnoreCase);
		int idx4b = lines.FindIndex((string l) => Regex.IsMatch(l, "^4B[\\. :]*", RegexOptions.IgnoreCase));
		if (idx4b >= 0)
		{
			int next;
			for (next = idx4b + 1; next < lines.Count && string.IsNullOrWhiteSpace(lines[next]); next++)
			{
			}
			if (next < lines.Count)
			{
				string lineAfter4b = lines[next];
				if (Regex.IsMatch(lineAfter4b, "^5[\\. ]*$", RegexOptions.IgnoreCase))
				{
					int after5;
					for (after5 = next + 1; after5 < lines.Count && string.IsNullOrWhiteSpace(lines[after5]); after5++)
					{
					}
					if (after5 < lines.Count)
					{
						Match match = licenceRegex.Match(lines[after5]);
						if (match.Success)
						{
							licenceNumber = match.Value;
						}
					}
				}
				else
				{
					Match match2 = licenceRegex.Match(lineAfter4b);
					if (match2.Success)
					{
						licenceNumber = match2.Value;
					}
				}
			}
		}
		if (licenceNumber == null)
		{
			Match fallback = licenceRegex.Match(text);
			if (fallback.Success)
			{
				licenceNumber = fallback.Value;
			}
		}
		string address = null;
		int f7Index = lines.FindIndex((string l) => Regex.IsMatch(l, "^7[\\. ]*$"));
		if (f7Index >= 0)
		{
			List<string> addrLines = new List<string>();
			for (int i = f7Index + 1; i < lines.Count && !Regex.IsMatch(lines[i], "^\\d[AB]?$"); i++)
			{
				addrLines.Add(lines[i]);
			}
			address = string.Join(", ", addrLines).Trim();
		}
		return new VerifiedDocument
		{
			document_number = licenceNumber,
			surname = surname,
			given_names = givenNames,
			nationality = "GBR",
			date_of_birth = dob,
			expiry_date = expiry,
			issue_date = issue,
			address = address,
			visa_type = null,
			is_valid = (licenceNumber != null)
		};
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

	private string ExtractAddress(string text)
	{
		Match match = Regex.Match(text, "ADDRESS\\s*[:\\-]?\\s*([\\s\\S]+)");
		if (match.Success)
		{
			return match.Groups[1].Value.Trim();
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Google_OCR;

public class UnifiedDocumentParserService
{
	public VerifiedDocument Parse(string documentType, string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return new VerifiedDocument
			{
				is_valid = false
			};
		}
		text = OcrNormalizer.Normalize(text);
		switch (documentType?.ToLowerInvariant())
		{
		case "passport":
			return ParsePassport(text);
		case "driving_licence":
			return ParseDrivingLicence(text);
		case "national_id":
			return ParseNationalId(text);
		case "residence_permit":
		case "brp":
			return ParseResidencePermit(text);
		case "india_aadhaar":
			return ParseIndiaAadhaar(text);
		case "india_pan":
			return ParseIndiaPan(text);
		case "india_voter_id":
			return ParseIndiaVoterId(text);
		default:
			return new VerifiedDocument
			{
				document_type = documentType,
				is_valid = false
			};
		}
	}

	public VerifiedDocument ParsePassport(string rawOcr)
	{
		VerifiedDocument result = new VerifiedDocument
		{
			document_type = "passport",
			is_valid = false
		};
		if (string.IsNullOrWhiteSpace(rawOcr))
		{
			return result;
		}
		(string, string)? mrzLines = ExtractAndCleanMrzLines(rawOcr);
		if (mrzLines.HasValue)
		{
			ParseIcaoTd3Mrz(mrzLines.Value.Item1, mrzLines.Value.Item2, result);
		}
		else
		{
			ParseVisualFallback(rawOcr, result);
		}
		result.is_valid = !string.IsNullOrWhiteSpace(result.document_number) && !string.IsNullOrWhiteSpace(result.surname);
		return result;
	}

	private static (string Line1, string Line2)? ExtractAndCleanMrzLines(string rawOcr)
	{
		string sanitized = rawOcr.ToUpperInvariant().Replace("(", "<").Replace(")", "<")
			.Replace("[", "<")
			.Replace("]", "<")
			.Replace("{", "<")
			.Replace("}", "<")
			.Replace("«", "<")
			.Replace("»", "<");
		string mrzOnlyText = Regex.Replace(sanitized, "[^A-Z0-9<\\n]", "");
		List<string> lines = (from x in mrzOnlyText.Split('\n')
			select x.Trim() into x
			where x.Length >= 30
			select x).ToList();
		for (int i = 0; i < lines.Count - 1; i++)
		{
			string l1 = lines[i];
			string l2 = lines[i + 1];
			if (l1.StartsWith("P") && l1.Contains("<"))
			{
				l1 = FixMrzLineLength(l1, 44);
				l2 = FixMrzLineLength(l2, 44);
				if (l1.Length == 44 && l2.Length == 44)
				{
					return (l1, l2);
				}
			}
		}
		string singleStream = Regex.Replace(sanitized, "[^A-Z0-9<]", "");
		Match match = Regex.Match(singleStream, "(P[A-Z0-9<]{43})([A-Z0-9<]{44})");
		if (match.Success)
		{
			return (match.Groups[1].Value, match.Groups[2].Value);
		}
		return null;
	}

	private static string FixMrzLineLength(string line, int targetLength)
	{
		if (line.Length == targetLength)
		{
			return line;
		}
		if (line.Length > targetLength)
		{
			return line.Substring(0, targetLength);
		}
		return line.PadRight(targetLength, '<');
	}

	private static void ParseIcaoTd3Mrz(string l1, string l2, VerifiedDocument r)
	{
		string issuingCountry = FixAlphaField(l1.Substring(2, 3));
		string nameSection = l1.Substring(5, 39);
		string[] nameParts = nameSection.Split(new string[1] { "<<" }, StringSplitOptions.None);
		r.surname = CleanNamePart(nameParts[0]);
		r.given_names = ((nameParts.Length > 1) ? CleanNamePart(nameParts[1]) : null);
		string rawDocNum = l2.Substring(0, 9);
		char docNumCheck = l2[9];
		r.document_number = ResolveNumberVsLetter(rawDocNum, docNumCheck).Replace("<", "").Trim();
		r.nationality = FixAlphaField(l2.Substring(10, 3));
		if (string.IsNullOrWhiteSpace(r.nationality) || r.nationality.Contains("<"))
		{
			r.nationality = issuingCountry;
		}
		string rawDob = l2.Substring(13, 6);
		char dobCheck = l2[19];
		string validDob = ResolveNumberVsLetter(rawDob, dobCheck);
		r.date_of_birth = FormatIcaoDate(validDob, isExpiry: false);
		string rawExpiry = l2.Substring(21, 6);
		char expiryCheck = l2[27];
		string validExpiry = ResolveNumberVsLetter(rawExpiry, expiryCheck);
		r.expiry_date = FormatIcaoDate(validExpiry, isExpiry: true);
		r.visa_type = l2[20] switch
		{
			'F' => "FEMALE", 
			'M' => "MALE", 
			_ => null, 
		};
	}

	private static int CalculateIcaoChecksum(string input)
	{
		int[] weights = new int[3] { 7, 3, 1 };
		int sum = 0;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			int value;
			if (c >= '0' && c <= '9')
			{
				value = c - 48;
			}
			else
			{
				value = ((c >= 'A' && c <= 'Z') ? (c - 65 + 10) : ((c != '<') ? 0 : 0));
			}
			sum += value * weights[i % 3];
		}
		return sum % 10;
	}

	private static bool ValidateIcaoChecksum(string field, char checkDigit)
	{
		if (!char.IsDigit(checkDigit))
		{
			return false;
		}
		int expected = checkDigit - 48;
		return CalculateIcaoChecksum(field) == expected;
	}

	private static string ResolveNumberVsLetter(string rawField, char checkDigit)
	{
		if (ValidateIcaoChecksum(rawField, checkDigit))
		{
			return rawField;
		}
		string fixedAsDigits = rawField.Replace('O', '0').Replace('Q', '0').Replace('I', '1')
			.Replace('L', '1')
			.Replace('Z', '2')
			.Replace('S', '5')
			.Replace('B', '8');
		ValidateIcaoChecksum(fixedAsDigits, checkDigit);
		return fixedAsDigits;
	}

	private static string FormatIcaoDate(string yyMmDd, bool isExpiry)
	{
		if (string.IsNullOrWhiteSpace(yyMmDd) || yyMmDd.Length < 6 || !yyMmDd.All(char.IsDigit))
		{
			return null;
		}
		int yy = int.Parse(yyMmDd.Substring(0, 2));
		int mm = int.Parse(yyMmDd.Substring(2, 2));
		int dd = int.Parse(yyMmDd.Substring(4, 2));
		if (mm < 1 || mm > 12 || dd < 1 || dd > 31)
		{
			return null;
		}
		int currentYear = DateTime.UtcNow.Year;
		int currentTwoDigitYear = currentYear % 100;
		int year;
		if (isExpiry)
		{
			year = ((yy <= currentTwoDigitYear + 20) ? (2000 + yy) : (1900 + yy));
		}
		else
		{
			year = ((2000 + yy <= currentYear) ? (2000 + yy) : (1900 + yy));
		}
		return $"{year:0000}-{mm:00}-{dd:00}";
	}

	private static string FixAlphaField(string input)
	{
		return input.Replace('0', 'O').Replace('1', 'I').Replace('5', 'S')
			.Replace('8', 'B')
			.Replace("<", "")
			.Trim();
	}

	private static string CleanNamePart(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return null;
		}
		string cleaned = FixAlphaField(raw).Replace("<", " ");
		return Regex.Replace(cleaned, "\\s+", " ").Trim();
	}

	private static void ParseVisualFallback(string text, VerifiedDocument r)
	{
		Match passportNoMatch = Regex.Match(text, "(?:PASSPORT\\s*NO|DOCUMENT\\s*NO|PASSEPORT|PASAPORTE)\\s*[:.\\-]?\\s*([A-Z0-9]{8,10})", RegexOptions.IgnoreCase);
		r.document_number = (passportNoMatch.Success ? passportNoMatch.Groups[1].Value : Regex.Match(text, "\\b[A-Z0-9]{8,9}\\b").Value);
		Match surnameMatch = Regex.Match(text, "(?:SURNAME|NOM|APELLIDOS)\\s*[:.\\-]?\\s*([A-Z\\s]+)", RegexOptions.IgnoreCase);
		if (surnameMatch.Success)
		{
			r.surname = surnameMatch.Groups[1].Value.Trim();
		}
		Match givenMatch = Regex.Match(text, "(?:GIVEN\\s*NAMES|PRENOMS|NOMBRES)\\s*[:.\\-]?\\s*([A-Z\\s]+)", RegexOptions.IgnoreCase);
		if (givenMatch.Success)
		{
			r.given_names = givenMatch.Groups[1].Value.Trim();
		}
	}

	private VerifiedDocument ParseNationalId(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "national_id",
			is_valid = false
		};
		List<string> lines = (from x in text.Split('\n')
			select x.Trim()).ToList();
		List<string> mrzLines = lines.Where((string x) => Regex.IsMatch(x, "^[I|C|A][A-Z0-9<]{28,31}")).Take(3).ToList();
		if (mrzLines.Count == 3)
		{
			string l1 = mrzLines[0].PadRight(30, '<');
			string l2 = mrzLines[1].PadRight(30, '<');
			string l3 = mrzLines[2].PadRight(30, '<');
			doc.nationality = l1.Substring(2, 3).Replace("<", "");
			doc.document_number = l1.Substring(5, 9).Replace("<", "");
			doc.date_of_birth = ParseMrzDate(l2.Substring(0, 6));
			doc.expiry_date = ParseMrzDate(l2.Substring(8, 6));
			string[] names = l3.Split("<<");
			doc.surname = names[0].Replace("<", " ").Trim();
			doc.given_names = ((names.Length > 1) ? names[1].Replace("<", " ").Trim() : null);
		}
		else
		{
			doc.document_number = ExtractRegex(text, "\\b[A-Z0-9]{8,12}\\b");
			doc.surname = ExtractLabelValue(text, "SURNAME|NOM|APELLIDO|NAAM|NACHNAME");
			doc.given_names = ExtractLabelValue(text, "GIVEN NAMES|PRENOM|NOMBRE|VOORNAAM|VORNAME");
			doc.date_of_birth = ExtractDate(text, "DATE OF BIRTH|FECHA DE NACIMIENTO|GEBURTSDATUM");
			doc.expiry_date = ExtractDate(text, "EXPIRY|VALIDEZ|GÜLTIG");
		}
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private VerifiedDocument ParseDrivingLicence(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "driving_licence",
			is_valid = false
		};
		if (text.Contains("ANSI ") || text.Contains("DLCLASS") || text.Contains("DCA"))
		{
			ParseAamvaBarcode(text, doc);
			if (doc.is_valid)
			{
				return doc;
			}
		}
		Match ukNumber = Regex.Match(text.Replace(" ", ""), "[A-Z]{5}\\d{6}[A-Z0-9]{5}");
		if (ukNumber.Success)
		{
			doc.document_number = ukNumber.Value;
			doc.nationality = "GBR";
		}
		else
		{
			doc.document_number = ExtractRegex(text, "(DL|LIC#|LICENSE|5\\.)\\s*[:#-]?\\s*([A-Z0-9]{6,15})", 2);
		}
		List<string> lines = (from x in text.Split('\n')
			select x.Trim()).ToList();
		Dictionary<string, string> fieldMap = BuildFieldMap(lines);
		doc.surname = GetFieldValue(fieldMap, "1") ?? ExtractLabelValue(text, "1\\.|LN|SURNAME|NOM");
		doc.given_names = CleanName(GetFieldValue(fieldMap, "2") ?? ExtractLabelValue(text, "2\\.|FN|GIVEN|PRENOM"));
		doc.date_of_birth = ParseDateFlexible(GetFieldValue(fieldMap, "3")) ?? ExtractDate(text, "3\\.|DOB");
		doc.issue_date = ParseDateFlexible(GetFieldValue(fieldMap, "4a")) ?? ExtractDate(text, "4a|ISSUE");
		doc.expiry_date = ParseDateFlexible(GetFieldValue(fieldMap, "4b")) ?? ExtractDate(text, "4b|EXPIRY|EXP");
		doc.address = GetFieldValue(fieldMap, "8") ?? ExtractAddress(text);
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private VerifiedDocument ParseResidencePermit(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "residence_permit",
			is_valid = false
		};
		List<string> lines = (from x in text.Split('\n')
			select x.Trim()).ToList();
		List<string> mrzLines = lines.Where((string x) => Regex.IsMatch(x, "^C[12][A-Z0-9<]{28}")).Take(3).ToList();
		if (mrzLines.Count == 3)
		{
			doc.nationality = "USA";
			doc.document_number = mrzLines[0].Substring(5, 9).Replace("<", "");
			doc.date_of_birth = ParseMrzDate(mrzLines[1].Substring(0, 6));
			doc.expiry_date = ParseMrzDate(mrzLines[1].Substring(8, 6));
			string[] names = mrzLines[2].Split("<<");
			doc.surname = names[0].Replace("<", " ").Trim();
			doc.given_names = ((names.Length > 1) ? names[1].Replace("<", " ").Trim() : null);
			doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
			return doc;
		}
		Match brpMatch = Regex.Match(text, "\\b[A-Z]{2}\\d{7}[A-Z]\\b");
		doc.document_number = (brpMatch.Success ? brpMatch.Value : ExtractRegex(text, "\\bUSCIS#\\s*[:#-]?\\s*(\\d{9})\\b", 1));
		doc.surname = ExtractLabelValue(text, "SURNAME|FAMILY NAME");
		doc.given_names = ExtractLabelValue(text, "GIVEN NAMES|FORENAMES");
		doc.nationality = ExtractLabelValue(text, "NATIONALITY");
		doc.date_of_birth = ExtractDate(text, "DOB|DATE OF BIRTH");
		doc.expiry_date = ExtractDate(text, "EXPIRY|VALID UNTIL|CARD EXPIRES");
		doc.visa_type = ExtractLabelValue(text, "TYPE|REMARKS|CATEGORY");
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private VerifiedDocument ParseIndiaAadhaar(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "india_aadhaar",
			nationality = "IND",
			is_valid = false
		};
		Match match = Regex.Match(text, "\\b\\d{4}\\s?\\d{4}\\s?\\d{4}\\b");
		if (match.Success)
		{
			doc.document_number = match.Value.Replace(" ", "");
		}
		doc.date_of_birth = ExtractDate(text, "DOB|DATE OF BIRTH|YEAR OF BIRTH");
		if (text.Contains("FEMALE"))
		{
			doc.visa_type = "FEMALE";
		}
		else if (text.Contains("MALE"))
		{
			doc.visa_type = "MALE";
		}
		List<string> lines = (from x in text.Split('\n')
			select x.Trim() into x
			where x.Length > 2
			select x).ToList();
		int dobIndex = lines.FindIndex((string x) => Regex.IsMatch(x, "DOB|DATE OF BIRTH|YEAR OF BIRTH", RegexOptions.IgnoreCase));
		if (dobIndex > 0)
		{
			doc.given_names = lines[dobIndex - 1];
		}
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private VerifiedDocument ParseIndiaPan(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "india_pan",
			nationality = "IND",
			is_valid = false
		};
		Match panMatch = Regex.Match(text, "\\b[A-Z]{5}\\d{4}[A-Z]\\b");
		if (panMatch.Success)
		{
			doc.document_number = panMatch.Value;
		}
		doc.date_of_birth = ExtractDate(text, "\\d{2}/\\d{2}/\\d{4}");
		List<string> lines = (from x in text.Split('\n')
			select x.Trim() into x
			where !string.IsNullOrWhiteSpace(x)
			select x).ToList();
		int panIdx = lines.FindIndex((string x) => Regex.IsMatch(x, "\\b[A-Z]{5}\\d{4}[A-Z]\\b"));
		if (panIdx >= 2)
		{
			doc.surname = lines[panIdx - 2];
			doc.given_names = lines[panIdx - 1];
		}
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private VerifiedDocument ParseIndiaVoterId(string text)
	{
		VerifiedDocument doc = new VerifiedDocument
		{
			document_type = "india_voter_id",
			nationality = "IND",
			is_valid = false
		};
		Match voterMatch = Regex.Match(text, "\\b[A-Z]{3}\\d{7}\\b");
		if (voterMatch.Success)
		{
			doc.document_number = voterMatch.Value;
		}
		doc.surname = ExtractLabelValue(text, "SURNAME|FAMILY NAME");
		doc.given_names = ExtractLabelValue(text, "NAME|ELECTOR NAME");
		doc.date_of_birth = ExtractDate(text, "DOB|AGE|DATE OF BIRTH");
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
		return doc;
	}

	private void ParseTd3Mrz(string mrz1, string mrz2, VerifiedDocument doc)
	{
		try
		{
			if (mrz2.Length >= 30)
			{
				doc.document_number = mrz2.Substring(0, 9).Replace("<", "").Trim();
				string nationality = mrz2.Substring(10, 3).Replace("<", "").Trim();
				string dobStr = mrz2.Substring(13, 6);
				doc.date_of_birth = FormatMrzDate(dobStr);
				string expiryStr = mrz2.Substring(21, 6);
				doc.expiry_date = FormatMrzDate(expiryStr);
			}
			if (mrz1.Length > 5)
			{
				string namesPart = mrz1.Substring(5);
				string[] nameParts = namesPart.Split(new string[1] { "<<" }, StringSplitOptions.None);
				if (nameParts.Length != 0)
				{
					doc.surname = nameParts[0].Replace("<", " ").Trim();
				}
				if (nameParts.Length > 1)
				{
					doc.given_names = nameParts[1].Replace("<", " ").Trim();
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("MRZ Parsing Error: " + ex.Message);
		}
	}

	private string FormatMrzDate(string yymmdd)
	{
		if (yymmdd.Length != 6 || !int.TryParse(yymmdd, out var _))
		{
			return yymmdd;
		}
		string yy = yymmdd.Substring(0, 2);
		string mm = yymmdd.Substring(2, 2);
		string dd = yymmdd.Substring(4, 2);
		int yearVal = int.Parse(yy);
		string century = ((yearVal >= 50) ? "19" : "20");
		return $"{dd}-{mm}-{century}{yy}";
	}

	private static void ParseAamvaBarcode(string rawText, VerifiedDocument doc)
	{
		doc.nationality = "USA";
		doc.document_number = ExtractAamvaField(rawText, "DAQ");
		doc.surname = ExtractAamvaField(rawText, "DCS");
		doc.given_names = ExtractAamvaField(rawText, "DAC");
		string rawDob = ExtractAamvaField(rawText, "DBB");
		string rawExpiry = ExtractAamvaField(rawText, "DBA");
		string rawIssue = ExtractAamvaField(rawText, "DBD");
		doc.date_of_birth = FormatAamvaDate(rawDob);
		doc.expiry_date = FormatAamvaDate(rawExpiry);
		doc.issue_date = FormatAamvaDate(rawIssue);
		string street = ExtractAamvaField(rawText, "DAG");
		string city = ExtractAamvaField(rawText, "DAI");
		string state = ExtractAamvaField(rawText, "DAJ");
		string zip = ExtractAamvaField(rawText, "DAK");
		doc.address = $"{street}, {city}, {state} {zip}".Trim(',', ' ');
		doc.is_valid = !string.IsNullOrWhiteSpace(doc.document_number);
	}

	private static string ExtractAamvaField(string text, string code)
	{
		Match match = Regex.Match(text, code + "([^\\r\\n]+)");
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value.Trim();
	}

	private static string FormatAamvaDate(string rawDate)
	{
		if (string.IsNullOrWhiteSpace(rawDate) || rawDate.Length < 8)
		{
			return null;
		}
		if (Regex.IsMatch(rawDate, "^\\d{8}$"))
		{
			string month = rawDate.Substring(0, 2);
			string day = rawDate.Substring(2, 2);
			string year = rawDate.Substring(4, 4);
			return $"{year}-{month}-{day}";
		}
		return null;
	}

	private static string ParseMrzDate(string v)
	{
		if (string.IsNullOrEmpty(v) || v.Length < 6 || !v.All(char.IsDigit))
		{
			return null;
		}
		int yy = int.Parse(v.Substring(0, 2));
		int mm = int.Parse(v.Substring(2, 2));
		int dd = int.Parse(v.Substring(4, 2));
		int currentTwoDigitYear = DateTime.UtcNow.Year % 100;
		int year = ((yy > currentTwoDigitYear + 20) ? (1900 + yy) : (2000 + yy));
		return $"{year:0000}-{mm:00}-{dd:00}";
	}

	private string ExtractDate(string text, string labelPattern)
	{
		Match match = Regex.Match(text, "(" + labelPattern + ")\\s*[:\\-]?\\s*(\\d{2}[.\\-/ ]\\d{2}[.\\-/ ]\\d{4})", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return ParseDateFlexible(match.Groups[2].Value);
		}
		Match fallback = Regex.Match(text, "\\b(\\d{2})[.\\-/ ](\\d{2})[.\\-/ ](\\d{4})\\b");
		if (!fallback.Success)
		{
			return null;
		}
		return ParseDateFlexible(fallback.Value);
	}

	private string ExtractLabelValue(string text, string labelPattern)
	{
		Match match = Regex.Match(text, "(" + labelPattern + ")\\s*[:\\-]?\\s*([A-Z0-9 ,\\-\\/]+)", RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[2].Value.Trim();
	}

	private string ExtractRegex(string text, string pattern, int groupIndex = 0)
	{
		Match match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[groupIndex].Value.Trim();
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

	private Dictionary<string, string> BuildFieldMap(List<string> lines)
	{
		Dictionary<string, string> map = new Dictionary<string, string>();
		string currentField = null;
		foreach (string line in lines)
		{
			Match m = Regex.Match(line, "^(1|2|3|4a|4b|4|5|8|9)[\\.\\s]", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				currentField = m.Groups[1].Value.ToLower();
				map[currentField] = line.Substring(m.Length).Trim();
			}
			else if (currentField != null && !Regex.IsMatch(line, "^\\d[\\.\\s]"))
			{
				Dictionary<string, string> dictionary = map;
				string key = currentField;
				dictionary[key] = dictionary[key] + " " + line;
			}
		}
		return map;
	}

	private string GetFieldValue(Dictionary<string, string> map, string key)
	{
		if (!map.TryGetValue(key, out string val))
		{
			return null;
		}
		return val.Trim();
	}

	private string CleanName(string name)
	{
		return name?.Replace(".", "").Replace(":", "").Trim();
	}

	private string ExtractAddress(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		string cleaned = Regex.Replace(text, "^(1|2|3|4a|4b|4|5|8)\\s.*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		cleaned = cleaned.Replace("&", " ").Replace("UK DRIVING LICENCE", " ");
		List<string> lines = (from x in cleaned.Split('\n')
			select x.Trim() into x
			where !string.IsNullOrWhiteSpace(x)
			select x).ToList();
		List<string> parts = lines.Where((string x) => !Regex.IsMatch(x, "^[A-Z]{1,2}\\d[A-Z\\d]?\\s*\\d[A-Z]{2}$") && !Regex.IsMatch(x, "^\\d+$")).ToList();
		return string.Join(" ", parts).Trim();
	}
}

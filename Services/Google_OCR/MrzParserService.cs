using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Google_OCR;

public class MrzParserService
{
	public ParsedMrz Parse(List<string> mrzLines)
	{
		if (mrzLines == null || mrzLines.Count < 2)
		{
			return new ParsedMrz
			{
				valid = false
			};
		}
		string line1 = Normalise(mrzLines[0]);
		string line2 = Normalise(mrzLines[1]);
		if (line2.Length < 30)
		{
			line2 = PadLine2(line2);
		}
		string surname = ExtractSurname(line1);
		string givenNames = ExtractGivenNames(line1);
		string passportNumber = ExtractPassportNumber(line2);
		string nationality = ExtractNationality(line2);
		string dob = ParseDate(line2.Substring(13, 6));
		string expiry = ParseDate(line2.Substring(21, 6));
		return new ParsedMrz
		{
			document_number = passportNumber,
			surname = surname,
			given_names = givenNames,
			nationality = nationality,
			date_of_birth = dob,
			expiry_date = expiry,
			valid = true
		};
	}

	private string Normalise(string line)
	{
		line = line.Replace(" ", "");
		line = line.Replace(".", "");
		line = line.Replace("=", "");
		line = Regex.Replace(line, "[^A-Z0-9<]", "");
		line = Regex.Replace(line, "<+", "<");
		return line.Trim();
	}

	private string PadLine2(string line2)
	{
		while (line2.Length < 44)
		{
			line2 += "<";
		}
		return line2;
	}

	private string ExtractSurname(string line1)
	{
		string[] parts = line1.Substring(2).Split(new string[1] { "<<" }, StringSplitOptions.None);
		return parts[0];
	}

	private string ExtractGivenNames(string line1)
	{
		string[] parts = line1.Substring(2).Split(new string[1] { "<<" }, StringSplitOptions.None);
		if (parts.Length <= 1)
		{
			return null;
		}
		return parts[1].Replace("<", " ").Trim();
	}

	private string ExtractPassportNumber(string line2)
	{
		return line2.Substring(0, 9).Replace("<", "");
	}

	private string ExtractNationality(string line2)
	{
		return line2.Substring(10, 3);
	}

	private string ParseDate(string yymmdd)
	{
		if (yymmdd.Length != 6)
		{
			return null;
		}
		string yy = yymmdd.Substring(0, 2);
		string mm = yymmdd.Substring(2, 2);
		string dd = yymmdd.Substring(4, 2);
		int year = ((int.Parse(yy) >= 50) ? (1900 + int.Parse(yy)) : (2000 + int.Parse(yy)));
		return $"{year}-{mm}-{dd}";
	}

	public List<string> ExtractMRZ(string text)
	{
		List<string> lines = (from l in text.Split('\n')
			select l.Trim() into l
			where l.Length > 0
			select l).ToList();
		string line1 = lines.FirstOrDefault((string l) => l.StartsWith("P<"));
		if (line1 == null)
		{
			return new List<string>();
		}
		string line2 = lines.Where((string l) => l != line1 && l.Count((char c) => c == '<') >= 10).FirstOrDefault();
		if (line2 == null)
		{
			List<string> parts = lines.Where((string l) => l.Any((char c) => c == '<')).ToList();
			if (parts.Count >= 2)
			{
				line2 = parts[0] + parts[1];
			}
		}
		return new List<string> { line1, line2 };
	}
}

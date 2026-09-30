using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TradePlatform.Api.Services.Google_OCR;

public class DocumentTypeService
{
	public string Detect(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "unknown";
		}
		string t = text.ToUpperInvariant();
		Dictionary<string, int> scores = new Dictionary<string, int>
		{
			{ "passport", 0 },
			{ "driving_licence", 0 },
			{ "national_id", 0 },
			{ "residence_permit", 0 },
			{ "india_aadhaar", 0 },
			{ "india_pan", 0 },
			{ "india_voter_id", 0 }
		};
		if (Regex.IsMatch(t, "P<[A-Z0-9<]{3}"))
		{
			scores["passport"] += 15;
		}
		if (t.Contains("PASSPORT") || t.Contains("PASSEPORT") || t.Contains("PASAPORTE"))
		{
			scores["passport"] += 8;
		}
		if (t.Contains("UNITED KINGDOM") || t.Contains("HMPO") || t.Contains("USA") || t.Contains("REPUBLIC OF INDIA"))
		{
			scores["passport"] += 2;
		}
		if (t.Contains("DRIVING LICENCE") || t.Contains("DRIVER LICENSE") || t.Contains("DRIVER'S LICENSE") || t.Contains("PERMIS DE CONDUIRE") || t.Contains("FÜHRERSCHEIN") || t.Contains("PATENTE DI GUIDA"))
		{
			scores["driving_licence"] += 12;
		}
		if (t.Contains("DVLA") || t.Contains("DMV") || t.Contains("DEPT OF MOTOR VEHICLES"))
		{
			scores["driving_licence"] += 8;
		}
		if (Regex.IsMatch(t, "[A-Z]{5}\\d{6}[A-Z0-9]{5}"))
		{
			scores["driving_licence"] += 8;
		}
		if (t.Contains("ANSI ") || t.Contains("DLCLASS") || t.Contains("DL CLASS"))
		{
			scores["driving_licence"] += 15;
		}
		if (Regex.IsMatch(t, "I[A-Z0-9<]<[A-Z]{3}"))
		{
			scores["national_id"] += 15;
		}
		if (t.Contains("NATIONAL IDENTITY CARD") || t.Contains("IDENTITY CARD") || t.Contains("CARTE D'IDENTITE") || t.Contains("TARJETA DE IDENTIDAD") || t.Contains("PERSONALAUSWEIS") || t.Contains("CARTA D'IDENTITÀ"))
		{
			scores["national_id"] += 10;
		}
		if (t.Contains("EUROPEAN UNION") || t.Contains("UNION EUROPÉENNE"))
		{
			scores["national_id"] += 5;
		}
		if (t.Contains("STATE IDENTIFICATION") || t.Contains("IDENTIFICATION CARD"))
		{
			scores["national_id"] += 8;
		}
		if (t.Contains("RESIDENCE PERMIT") || t.Contains("BIOMETRIC RESIDENCE") || t.Contains("TITRE DE SEJOUR") || t.Contains("AUFENTHALTSTITEL"))
		{
			scores["residence_permit"] += 12;
		}
		if (t.Contains("PERMANENT RESIDENT") || t.Contains("USCIS#") || t.Contains("USCIS") || Regex.IsMatch(t, "^C[12][A-Z0-9<]{28}"))
		{
			scores["residence_permit"] += 12;
		}
		if (t.Contains("UKVI"))
		{
			scores["residence_permit"] += 6;
		}
		if (t.Contains("UNIQUE IDENTIFICATION AUTHORITY OF INDIA") || t.Contains("AADHAAR") || t.Contains("UIDAI"))
		{
			scores["india_aadhaar"] += 12;
		}
		if (t.Contains("GOVERNMENT OF INDIA") && (t.Contains("MALE") || t.Contains("FEMALE")))
		{
			scores["india_aadhaar"] += 4;
		}
		if (Regex.IsMatch(t, "\\b\\d{4}\\s?\\d{4}\\s?\\d{4}\\b"))
		{
			scores["india_aadhaar"] += 10;
		}
		if (t.Contains("INCOME TAX DEPARTMENT") || t.Contains("PERMANENT ACCOUNT NUMBER"))
		{
			scores["india_pan"] += 12;
		}
		if (Regex.IsMatch(t, "\\b[A-Z]{5}\\d{4}[A-Z]\\b"))
		{
			scores["india_pan"] += 12;
		}
		if (t.Contains("ELECTION COMMISSION OF INDIA") || t.Contains("ELECTOR PHOTO IDENTITY CARD"))
		{
			scores["india_voter_id"] += 12;
		}
		if (Regex.IsMatch(t, "\\b[A-Z]{3}\\d{7}\\b"))
		{
			scores["india_voter_id"] += 10;
		}
		KeyValuePair<string, int> topMatch = scores.OrderByDescending((KeyValuePair<string, int> x) => x.Value).FirstOrDefault();
		if (topMatch.Value < 8)
		{
			return "unknown";
		}
		return topMatch.Key;
	}
}

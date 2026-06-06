using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Azure_OCR
{
    public class UnifiedDocumentParserService
    {
        private readonly MrzParserService _mrz;

        public UnifiedDocumentParserService(MrzParserService mrz)
        {
            _mrz = mrz;
        }

        public VerifiedDocument Parse(string documentType, string text)
        {
            switch (documentType)
            {
                case "passport":
                    return ParsePassport(text);

                case "driving_licence":
                    return ParseDrivingLicence(text);

                case "brp":
                    return ParseBrp(text);

                default:
                    return new VerifiedDocument { is_valid = false };
            }
        }

        // PASSPORT (MRZ)
        private VerifiedDocument ParsePassport(string text)
        {
            var mrz = _mrz.ExtractMRZ(text);
            var parsed = _mrz.Parse(mrz);

            return new VerifiedDocument
            {
                document_number =parsed.document_number,
                surname = parsed.surname,
                given_names = parsed.given_names,
                nationality = parsed.nationality,
                date_of_birth = parsed.date_of_birth,
                expiry_date = parsed.expiry_date,
                issue_date = (string)null,
                address = (string)null,
                visa_type = (string)null,
                is_valid = parsed.valid
            };
        }

        // UK DRIVING LICENCE
        // UK DRIVING LICENCE (PRODUCTION READY)
        private VerifiedDocument ParseDrivingLicence(string text)
        {
            var upper = text.ToUpperInvariant();
            var lines = upper.Split('\n').Select(l => l.Trim()).ToList();

            // -----------------------------
            // FIELD 1 — SURNAME
            // -----------------------------
            string surname = null;
            var f1 = Regex.Match(upper, @"\b1[\. ]+\s*([A-Z]+)\b");
            if (f1.Success)
                surname = f1.Groups[1].Value;

            // -----------------------------
            // FIELD 2 — GIVEN NAMES (2, 2., @, @.)
            // -----------------------------
            string givenNames = null;

            // Try "2" formats
            var f2 = Regex.Match(upper, @"\b2[\. ]+\s*([A-Z ]+)");
            if (f2.Success)
                givenNames = CleanName(f2.Groups[1].Value);

            // Try "@"
            if (givenNames == null)
            {
                var at = Regex.Match(upper, @"@\s*([A-Z ]+)");
                if (at.Success)
                    givenNames = CleanName(at.Groups[1].Value);
            }

            // Fallback: second uppercase line after surname
            if (givenNames == null)
            {
                var caps = Regex.Matches(upper, @"\b[A-Z]{2,}\b");
                if (caps.Count > 1)
                    givenNames = caps[1].Value;
            }

            // -----------------------------
            // FIELD 3 — DOB
            // -----------------------------
            string dob = null;
            var f3 = Regex.Match(upper, @"\b3[\. ]+\s*(\d{2})[.\-/ ](\d{2})[.\-/ ](\d{4})");
            if (f3.Success)
                dob = $"{f3.Groups[3].Value}-{f3.Groups[2].Value}-{f3.Groups[1].Value}";

            // -----------------------------
            // FIELD 4a — ISSUE DATE
            // -----------------------------
            string issue = null;
            var f4a = Regex.Match(upper, @"4A[\. ]+\s*(\d{2})[.\-/ ](\d{2})[.\-/ ](\d{4})");
            if (f4a.Success)
                issue = $"{f4a.Groups[3].Value}-{f4a.Groups[2].Value}-{f4a.Groups[1].Value}";

            // -----------------------------
            // FIELD 4b — EXPIRY DATE
            // -----------------------------
            string expiry = null;
            var f4b = Regex.Match(upper, @"4B[\. ]+\s*(\d{2})[.\-/ ](\d{2})[.\-/ ](\d{4})");
            if (f4b.Success)
                expiry = $"{f4b.Groups[3].Value}-{f4b.Groups[2].Value}-{f4b.Groups[1].Value}";

            // -----------------------------
            // LICENCE NUMBER — STRICT DVLA LOGIC
            // -----------------------------
            string licenceNumber = null;

            // DVLA licence number pattern (no spaces)
            var licenceRegex = new Regex(@"\b[A-Z]{5}\d{6}[A-Z]\d{2}[A-Z]{2}\b", RegexOptions.IgnoreCase);

            // Find the line containing "4b"
            int idx4b = lines.FindIndex(l => Regex.IsMatch(l, @"^4B[\. :]*", RegexOptions.IgnoreCase));

            if (idx4b >= 0)
            {
                // Line immediately after 4b
                int next = idx4b + 1;

                // Skip blank lines
                while (next < lines.Count && string.IsNullOrWhiteSpace(lines[next]))
                    next++;

                if (next < lines.Count)
                {
                    string lineAfter4b = lines[next];

                    // CASE 1 — If the next line is "5" or "5."
                    if (Regex.IsMatch(lineAfter4b, @"^5[\. ]*$", RegexOptions.IgnoreCase))
                    {
                        // Licence number MUST be on the next non-empty line
                        int after5 = next + 1;

                        while (after5 < lines.Count && string.IsNullOrWhiteSpace(lines[after5]))
                            after5++;

                        if (after5 < lines.Count)
                        {
                            var match = licenceRegex.Match(lines[after5]);
                            if (match.Success)
                                licenceNumber = match.Value;
                        }
                    }
                    else
                    {
                        // CASE 2 — If the next line is NOT 5 → try to extract licence number from that line
                        var match = licenceRegex.Match(lineAfter4b);
                        if (match.Success)
                            licenceNumber = match.Value;
                    }
                }
            }

            // CASE 3 — Fallback: scan entire text if still null
            if (licenceNumber == null)
            {
                var fallback = licenceRegex.Match(text);
                if (fallback.Success)
                    licenceNumber = fallback.Value;
            }




            // -----------------------------
            // FIELD 7 — ADDRESS (multi-line)
            // -----------------------------
            string address = null;
            var f7Index = lines.FindIndex(l => Regex.IsMatch(l, @"^7[\. ]*$"));
            if (f7Index >= 0)
            {
                var addrLines = new List<string>();
                for (int i = f7Index + 1; i < lines.Count; i++)
                {
                    if (Regex.IsMatch(lines[i], @"^\d[AB]?$")) break;
                    addrLines.Add(lines[i]);
                }
                address = string.Join(", ", addrLines).Trim();
            }

            return new VerifiedDocument
            {
                document_number = licenceNumber,
                surname= surname,
                given_names= givenNames,
                nationality = "GBR",
                date_of_birth = dob,
                expiry_date = expiry,
                issue_date = issue,
                address=address,
                visa_type = (string)null,
                is_valid = licenceNumber != null
            };
        }

        private string CleanName(string name)
        {
            return name.Replace(".", "").Replace(":", "").Trim();
        }




        // BRP
        private VerifiedDocument ParseBrp(string text)
        {
            var upper = text.ToUpperInvariant();

            // BRP number: 2 letters + 7 digits + 1 letter
            var brpMatch = Regex.Match(upper, @"\b[A-Z]{2}\d{7}[A-Z]\b");
            var brpNumber = brpMatch.Success ? brpMatch.Value : null;

            var dobMatch = Regex.Match(upper, @"(DOB|DATE OF BIRTH)\s*[:\-]?\s*(\d{2}[\/\-]\d{2}[\/\-]\d{4})");
            var dob = dobMatch.Success ? dobMatch.Groups[2].Value : null;

            var expiryMatch = Regex.Match(upper, @"(EXPIRY|VALID UNTIL|EXP)\s*[:\-]?\s*(\d{2}[\/\-]\d{2}[\/\-]\d{4})");
            var expiry = expiryMatch.Success ? expiryMatch.Groups[2].Value : null;

            var visaMatch = Regex.Match(upper, @"(TYPE|REMARKS|CATEGORY)\s*[:\-]?\s*([A-Z0-9 \-/]+)");
            var visaType = visaMatch.Success ? visaMatch.Groups[2].Value.Trim() : null;

            var surname = ExtractLabelValue(upper, "SURNAME|FAMILY NAME");
            var givenNames = ExtractLabelValue(upper, "GIVEN NAMES|FORENAMES|FIRST NAMES");
            var nationality = ExtractLabelValue(upper, "NATIONALITY");

            return new VerifiedDocument
            {
                document_number = brpNumber,
                surname = surname,
                given_names = givenNames,
                nationality = nationality,               
                date_of_birth = dob,
                expiry_date = expiry,
                issue_date = (string)null,
                address = (string)null,
                visa_type = (string)null,
                is_valid = brpNumber != null && dob != null && expiry != null
            };
        }

        private string ExtractLabelValue(string text, string labelPattern)
        {
            var match = Regex.Match(text, $@"({labelPattern})\s*[:\-]?\s*([A-Z0-9 ,\-\/]+)");
            return match.Success ? match.Groups[2].Value.Trim() : null;
        }

        private string ExtractAddress(string text)
        {
            var match = Regex.Match(text, @"ADDRESS\s*[:\-]?\s*([\s\S]+)");
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }
    }
}

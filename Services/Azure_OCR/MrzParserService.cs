using System.Text.RegularExpressions;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Services.Azure_OCR
{
    public class MrzParserService
    {
        // ---------------------------------------------------------
        // MAIN ENTRY POINT (OLD SIGNATURE PRESERVED)
        // ---------------------------------------------------------
        public ParsedMrz Parse(List<string> mrzLines)
        {
            if (mrzLines == null || mrzLines.Count < 2)
                return new ParsedMrz { valid = false };

            // Normalise MRZ lines
            string line1 = Normalise(mrzLines[0]);
            string line2 = Normalise(mrzLines[1]);

            // Fix short or broken MRZ line 2
            if (line2.Length < 30)
                line2 = PadLine2(line2);

            // Extract MRZ fields
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

        // ---------------------------------------------------------
        // MRZ NORMALISATION
        // ---------------------------------------------------------
        private string Normalise(string line)
        {
            line = line.Replace(" ", "");
            line = line.Replace(".", "");
            line = line.Replace("=", "");
            line = Regex.Replace(line, @"[^A-Z0-9<]", "");
            line = Regex.Replace(line, @"<+", "<");
            return line.Trim();
        }

        private string PadLine2(string line2)
        {
            while (line2.Length < 44)
                line2 += "<";
            return line2;
        }

        // ---------------------------------------------------------
        // FIELD EXTRACTION
        // ---------------------------------------------------------
        private string ExtractSurname(string line1)
        {
            var parts = line1.Substring(2).Split(new[] { "<<" }, StringSplitOptions.None);
            return parts[0];
        }

        private string ExtractGivenNames(string line1)
        {
            var parts = line1.Substring(2).Split(new[] { "<<" }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1].Replace("<", " ").Trim() : null;
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
            if (yymmdd.Length != 6) return null;

            string yy = yymmdd.Substring(0, 2);
            string mm = yymmdd.Substring(2, 2);
            string dd = yymmdd.Substring(4, 2);

            int year = int.Parse(yy) >= 50 ? 1900 + int.Parse(yy) : 2000 + int.Parse(yy);

            return $"{year}-{mm}-{dd}";
        }

        // ---------------------------------------------------------
        // MRZ EXTRACTOR (YOU STILL CALL THIS BEFORE Parse)
        // ---------------------------------------------------------
        public List<string> ExtractMRZ(string text)
        {
            var lines = text
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            // MRZ line 1 ALWAYS starts with P<
            var line1 = lines.FirstOrDefault(l => l.StartsWith("P<"));
            if (line1 == null) return new List<string>();

            // MRZ line 2 ALWAYS contains many < characters
            var line2 = lines
                .Where(l => l != line1 && l.Count(c => c == '<') >= 10)
                .FirstOrDefault();

            // Azure OCR often splits MRZ line 2 into two lines
            if (line2 == null)
            {
                var parts = lines.Where(l => l.Any(c => c == '<')).ToList();
                if (parts.Count >= 2)
                    line2 = parts[0] + parts[1];
            }

            return new List<string> { line1, line2 };
        }
    }
}

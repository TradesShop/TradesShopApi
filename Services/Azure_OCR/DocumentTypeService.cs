using System.Text.RegularExpressions;

namespace TradePlatform.Api.Services.Azure_OCR
{
    public class DocumentTypeService
    {
        public string Detect(string text)
        {
            var upper = text.ToUpperInvariant();

            if (upper.Contains("P<") || upper.Contains("PASSPORT"))
                return "passport";

            if (upper.Contains("DRIVING LICENCE") ||
                upper.Contains("DRIVER LICENCE") ||
                Regex.IsMatch(upper, @"\bDRIVING\b") &&
                Regex.IsMatch(upper, @"\bLICEN[CS]E\b"))
                return "driving_licence";

            if (upper.Contains("BIOMETRIC RESIDENCE PERMIT") ||
                upper.Contains("BRP") ||
                upper.Contains("RESIDENCE PERMIT"))
                return "brp";

            return "unknown";
        }
    }
}

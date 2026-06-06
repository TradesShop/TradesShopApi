namespace TradePlatform.Api.Models.document
{
    public class VerifiedDocument
    {
      
        public Guid user_id { get; set; }

        public string? document_type { get; set; }

        public string? document_number { get; set; }
        public string? surname { get; set; }
        public string? given_names { get; set; }
        public string? nationality { get; set; }
        public string? date_of_birth { get; set; }
        public string? expiry_date { get; set; }

        public string? issue_date { get; set; }
        public string? address { get; set; }
        public string? visa_type { get; set; }

        public bool is_valid { get; set; }
        public string raw_text { get; set; }
        public DateTime verified_at { get; set; }
    }
}

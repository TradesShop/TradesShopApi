namespace TradePlatform.Api.Models.document
{
    public class ParsedDocument
    {
        public string document_number { get; set; }
        public string surname { get; set; }
        public string given_names { get; set; }
        public string nationality { get; set; }
        public string date_of_birth { get; set; }
        public string expiry_date { get; set; }
        public string issue_date { get; set; }
        public string address { get; set; }
        public string visa_type { get; set; }
        public bool valid { get; set; }


    }
}

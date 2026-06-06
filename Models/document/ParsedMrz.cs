namespace TradePlatform.Api.Models.document
{
    public class ParsedMrz
    {
        public string document_number { get; set; }
        public string surname { get; set; }
        public string given_names { get; set; }
        public string nationality { get; set; }
        public string date_of_birth { get; set; }
        public string expiry_date { get; set; }
        public bool valid { get; set; }
    }
}

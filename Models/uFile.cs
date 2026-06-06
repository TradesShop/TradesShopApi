namespace TradePlatform.Api.Models
{
    public class uFile
    {
        public Guid id { get; set; }        
        public string file_name { get; set; }
        public string file_url { get; set; }
        public string file_type { get; set; }
        public int size_kb { get; set; }
        public DateTime created_at { get; set; }
        public string description { get; set; }
    }
}

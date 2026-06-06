namespace TradePlatform.Api.DTOs
{
    public class FileDeleteRequestDto
    {
        public Guid id { get; set; }
        public string file_name { get; set; }
        //public Guid entity_id { get; set; }
        //public string entity_type { get; set; }
        //public string upload_type { get; set; }
    }
}

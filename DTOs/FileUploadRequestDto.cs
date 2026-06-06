namespace TradePlatform.Api.DTOs
{
    public class FileUploadRequestDto
    {
        public Guid entity_id { get; set; }
        public int entity_type { get; set; }
        public string upload_type { get; set; }
        public string filename { get; set; }
        public string content_type { get; set; }
        public string? work_stage { get; set; }
    }
}

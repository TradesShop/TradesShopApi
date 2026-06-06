namespace TradePlatform.Api.DTOs.Files
{
    public class FilesGetRequestDto
    {
        public Guid entity_id { get; set; }
        public int entity_type { get; set; }
        public string upload_type { get; set; }

    }
}

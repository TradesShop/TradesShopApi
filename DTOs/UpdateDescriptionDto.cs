namespace TradePlatform.Api.DTOs
{
    public class UpdateDescriptionDto
    {
        public Guid file_id { get; set; }
        public string description { get; set; }
    }
}

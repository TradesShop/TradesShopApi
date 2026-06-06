namespace TradePlatform.Api.DTOs
{
    public class BusinessProfileUpsertDto
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public int? active_since { get; set; }
        public string? website_url { get; set; }
        public int? business_type_id { get; set; }
        public int? number_of_employees { get; set; }
        public string? registration_number { get; set; }
        public int? service_radius_km { get; set; }
        public bool verified { get; set; }
    }
}

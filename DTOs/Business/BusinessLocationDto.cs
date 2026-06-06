namespace TradePlatform.Api.DTOs.Business
{
    public class BusinessLocationDto
    {
        public int id { get; set; }
        public Guid business_id { get; set; }
        public int address_id { get; set; }
        public bool is_primary { get; set; }
        public int? location_type_id { get; set; }
        public int service_radius_km { get; set; }

        public string address_line1 { get; set; }
        public string address_line2 { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
    }

}

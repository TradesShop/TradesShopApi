namespace TradePlatform.Api.Models.Locations
{
    
        public class locations_sitemap
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
            public string slug { get; set; } = string.Empty;
            public DateTime? updated_at { get; set; }
    }
    public class LocationSummary
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string region { get; set; } = string.Empty;
        public string county { get; set; } = string.Empty;
        public List<string> postcodes { get; set; } = new();
    }
    public class ServiceLocation
    {
        public int? id { get; set; }

        public string slug { get; set; }

        public string name { get; set; }

        public string location_type { get; set; }

        public int? parent_location_id { get; set; }

        public string country { get; set; }

        public string county { get; set; }

        public string region { get; set; }

        public string postcode_prefixes { get; set; }

        public string aliases_json { get; set; }

        public double? latitude { get; set; }

        public double? longitude { get; set; }

        public int? population { get; set; }

        public bool? is_active { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }

   
}

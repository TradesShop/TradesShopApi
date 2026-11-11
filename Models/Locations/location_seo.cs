namespace TradePlatform.Api.Models.Locations
{
    public class location_seo
    {
        public int id { get; set; }
        public string slug { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string location_type { get; set; } = string.Empty;
        public int? parent_location_id { get; set; }
        public string country { get; set; } = "England";
        public string county { get; set; } = string.Empty;
        public string region { get; set; } = string.Empty;
        public object? postcode_prefixes { get; set; }
        public object? aliases { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public int? population { get; set; }
        public bool is_active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        // --- Location SEO Columns ---

        public string? seo_overview { get; set; }
        public string? location_story { get; set; }
        public string? local_history { get; set; }
        public string? local_economy { get; set; }
        public object? areas_json { get; set; }
        public object? nearby_locations_json { get; set; }
        public object? neighbouring_towns_json { get; set; }
        public object? landmarks_json { get; set; }
        public object? property_types_json { get; set; }
        public object? housing_characteristics_json { get; set; }
        public object? property_age_characteristics { get; set; }
        public object? commercial_areas_json { get; set; }
        public object? common_home_issues_json { get; set; }
        public string? service_demand_context { get; set; }
        public object? travel_connections_json { get; set; }
        public object? nearby_postcodes_json { get; set; }
        public object? weather_considerations_json { get; set; }
        public object? demographic_profile_json { get; set; }
        public string? local_authority { get; set; }
        public object? notable_neighbourhoods_json { get; set; }
        public string? faq_context { get; set; }
        public object? seo_keywords_json { get; set; }
        public string? ai_location_brief { get; set; }
        public DateTime? content_last_verified { get; set; }

        public bool is_alias_match { get; set; }
    }
}

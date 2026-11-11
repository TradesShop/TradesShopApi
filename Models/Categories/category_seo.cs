namespace TradePlatform.Api.Models.Categories
{
    public class category_seo
    {
        public int? id { get; set; }

        public int category_id { get; set; }
        public string category_slug { get; set; }
        public string category_overview { get; set; }
        public string? seo_overview { get; set; }
        public string alias_slugs { get; set; }
        public string seo_title_template { get; set; }

        public string seo_description_template { get; set; }

        public string canonical_url_template { get; set; }

        public string meta_robots { get; set; }

        public string h1_template { get; set; }

        public string intro_content { get; set; }

        public string local_content_template { get; set; }

        public string services_heading { get; set; }

        public string services_json { get; set; }

        public string related_keywords { get; set; }

        public string why_choose_heading { get; set; }

        public string why_choose_content { get; set; }

        public string faq_template_json { get; set; }

        public string og_title_template { get; set; }

        public string og_description_template { get; set; }

        public string og_image_url { get; set; }

        public bool? is_active { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }
    public class category_services
    {
        public int category_id { get; set; }
        public string category_name { get; set; } = string.Empty;
        public string primary_slug { get; set; } = string.Empty;
        public string? seo_overview { get; set; }
        public string? services_heading { get; set; }
        public string? services_json { get; set; }
        public string? related_keywords { get; set; }
    }

    public class category_meta_seo: category_seo
    {
        public int seo_id { get; set; }
        public string category_name { get; set; }
        public string primary_slug { get; set; }
    }

}

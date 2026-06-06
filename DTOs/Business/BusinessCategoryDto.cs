namespace TradePlatform.Api.DTOs.Business
{
    public class BusinessCategoryDto
    {
        public Guid id { get; set; }
        public Guid business_id { get; set; }
        public int category_id { get; set; }
        public string category_name { get; set; }
        public bool is_primary { get; set; }
        public DateTime created_at { get; set; }

    }
}

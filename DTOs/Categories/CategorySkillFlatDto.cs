namespace TradePlatform.Api.DTOs.Categories
{
    public class CategorySkillFlatDto
    {
        public int category_id { get; set; }
        public string category_name { get; set; } = string.Empty;

        public int? skill_id { get; set; }
        public string? skill_name { get; set; }
    }
}

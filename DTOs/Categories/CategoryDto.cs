namespace TradePlatform.Api.DTOs.Categories
{
    public class CategoryDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<CategorySkillDto> children { get; set; } = new();
    }
}

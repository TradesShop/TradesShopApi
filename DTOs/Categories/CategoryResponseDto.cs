namespace TradePlatform.Api.DTOs.Categories
{
    public class CategoryResponseDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;

        public List<CategoryChildDto> children { get; set; } = [];
    }
}

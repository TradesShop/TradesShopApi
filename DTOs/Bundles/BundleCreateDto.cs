namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundleCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public int ExpiryMonths { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

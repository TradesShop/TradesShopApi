namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundleUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int ExpiryMonths { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundlePriceCreateDto
    {
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string StripePriceId { get; set; } = string.Empty;
        public bool IsVatable { get; set; } = true;
    }
}

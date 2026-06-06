namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundlePriceDto
    {
        public Guid Id { get; set; }
        public Guid BundleId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string StripePriceId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsVatable { get; set; }
    }
}

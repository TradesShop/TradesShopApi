namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundleSelectDto
    {

        public Guid? target_user_id { get; set; }
        public Guid bundle_id { get; set; }
        public Guid bundle_price_id { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.Bundles
{
    public class BundleCheckoutCompletedDto
    {
        public Guid bundle_order_id { get; set; }
        public Guid bundle_price_id { get; set; }
        public Guid user_id { get; set; }

        public string stripe_payment_intent_id { get; set; }
        public string stripe_customer_id { get; set; }
        public string customer_email { get; set; }

        public decimal amount_total { get; set; }
        public decimal amount_subtotal { get; set; }
        public string currency { get; set; }
        public string metadataJson { get; set; }
    }
}

namespace TradePlatform.Api.Models
{
    public class PaymentsM
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid invoice_id { get; set; }
        public string stripe_payment_intent_id { get; set; }
        public string stripe_charge_id { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
    }
}

namespace TradePlatform.Api.Models
{
    public class PaymentMethod_db
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }

        public string stripe_payment_method_id { get; set; }
        public string? brand { get; set; }
        //public string? displaybrand { get; set; }
        public string? last4 { get; set; }
        public int? exp_month { get; set; }
        public int? exp_year { get; set; }
    
        public bool is_default { get; set; }
 
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Guid? updated_by { get; set; }
        public string name_on_card { get; set; }
    }
}

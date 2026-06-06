namespace TradePlatform.Api.Models
{
    public class UserInvoices
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }

        public string stripe_invoiceid { get; set; } = null!;
        public string? stripe_paymentintentid { get; set; }

        public decimal amount { get; set; }
        public string currency { get; set; } = null!;

        public string status { get; set; } = null!;

        public DateTime? invoice_date { get; set; }
        public DateTime? due_date { get; set; }
        public DateTime? paid_at { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
    public class AuthResponse
    {
        public string token { get; set; }
        public object User { get; set; }
    }
}

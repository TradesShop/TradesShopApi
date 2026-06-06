namespace TradePlatform.Api.Models
{
    public class UserCreditBalance
    {
        public Guid user_id { get; set; }
        public int balance { get; set; }
        public DateTime updated_at { get; set; }
    }
}

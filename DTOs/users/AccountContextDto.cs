namespace TradePlatform.Api.DTOs.users
{
    public class AccountContextDto
    {
        public Guid user_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public Guid business_id { get; set; }
        public bool email_verified { get; set; }
        public bool phone_verified { get; set; }
        public bool is_active { get; set; }
        public bool business_verified { get; set; }
        public bool identity_verified { get; set; }
        public int credits { get; set; }
        public bool has_payment_method { get; set; }
        public string default_intro_msg { get; set; }
    }
        
}

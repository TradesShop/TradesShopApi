namespace TradePlatform.Api.Models
{
    public enum UserType
    {
        
        customer = 1,
        tradesperson = 2,
        customerservice=3,
        admin = 9,
    }

    public class User
    {
        public Guid id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string? password_hash { get; set; }
        public string phone { get; set; }
        public int? user_type { get; set; }      
        public string? utype_code { get; set; }
        public bool isactive { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string? stripe_customer_id { get; set; }
        public bool? verified { get; set; }
        public Guid? customer_id { get; set; }
        public Guid? business_id { get; set; }
        public string? jwttoken { get; set; }
        public string? verifycode { get; set; }
    }
   
    public class UserMeta:User
    {
        public string postcode { get; set; }
        public decimal gLng { get; set; }
        public decimal gLat { get; set; }       

    }

    public class RegisterResponse
    {
        public string? token { get; set; }
        public string? refresh_token { get; set; } = null!;
        public string message { get; set; }
        public object? User { get; set; }

    }

}

namespace TradePlatform.Api.DTOs
{
    public class RegisterDto
    {
        public Guid? user_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string? business_name { get; set; }
        public int primarytrade { get; set; }
        public int secondarytrade { get; set; }
        public string email { get; set; }
        public string password_hash  { get; set; }
        public int user_type { get; set; }   //0=admin, 1=customer, 2=tradesperson,3=customerservice,
        public string phone { get; set; }
        public string postcode { get; set; }
        public int location_type_id { get; set; }
        public int? country_id { get; set; }
        public decimal longitude { get; set; }
        public decimal latitude { get; set; }
        public bool same_as_business { get; set; }        
        public string verifycode { get; set; }
        public string account_type { get; set; }

        public string? address_line1 { get; set; }
        public string? address_line2 { get; set; }
        public string? town { get; set; }
        public string? county { get; set; }
        public string? public_slug { get; set; }



    }
}

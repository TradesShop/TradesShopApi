namespace TradePlatform.Api.Models.Business
{
    public class BusinessProfile
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string website_url { get; set; }
        public int business_type_id { get; set; }
        public int number_of_employees { get; set; }
        public string registration_number { get; set; }
        public bool business_verified { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }        
        public bool identity_verified { get; set; }
        public string default_intro_message { get; set; }
        public Guid business_id { get; set; }
       
    }
}

namespace TradePlatform.Api.Models
{
    public class Tradesperson
    {
        public Guid Id { get; set; }
        public Guid User_Id { get; set; }
        public string Company_Name { get; set; }
        public string Bio { get; set; }
        public int? Years_Experience { get; set; }
        public string Postcode { get; set; }
        public string Address { get; set; }
        public int Country_Id { get; set; }
        public decimal? GLng { get; set; }
        public decimal? GLat { get; set; }
        public string Public_Liability_Insurance { get; set; }
        public bool Verified { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}

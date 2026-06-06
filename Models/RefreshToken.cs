namespace TradePlatform.Api.Models
{
    public class RefreshToken
    {
        public int id { get; set; }
        public Guid user_id { get; set; }
        public string token { get; set; }
        public DateTime expires_at { get; set; }
        public bool isrevoked { get; set; }
    }
}

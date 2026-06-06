namespace TradePlatform.Api.DTOs.Business
{
    public class BusinessWebProfileGetDto
    {
        public int id { get; set; }
        public Guid business_id { get; set; }
        public string platform { get; set; }
        public string url { get; set; }
        public DateTime created_at { get; set; }
    }
}

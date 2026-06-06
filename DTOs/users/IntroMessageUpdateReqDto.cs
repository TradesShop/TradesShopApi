namespace TradePlatform.Api.DTOs.users
{
    public class IntroMessageUpdateReqDto
    {
        public Guid? user_id { get; set; }
        public string default_intro_message { get; set; }
        
    }
}

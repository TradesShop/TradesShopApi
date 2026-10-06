namespace TradePlatform.Api.DTOs
{
    public class SendEmailCodeDto
    {
        public string email { get; set; }
    }

    public class VerifyEmailCodeDto
    {
        public string email { get; set; }
        public string code { get; set; }
    }
    public class SendVerificationCodeDto
    {
        public string? email { get; set; }
        public string? phonenumber { get; set; }
    }
}

namespace TradePlatform.Api.DTOs.users
{
    public class ChangePasswordDto
    {
        public string old_password { get; set; }
        public string new_password { get; set; }
    }
}

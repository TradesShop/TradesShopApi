namespace TradePlatform.Api.DTOs.users
{
    
        public class UserDto
        {
            public Guid id { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string email { get; set; }           
            public string phone { get; set; }
            public int? user_type { get; set; }
            public string? password_hash { get; set; }
             public string? verifycode { get; set; }
    }
}

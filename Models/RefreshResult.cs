namespace TradePlatform.Api.Models
{
    public class RefreshResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        public static RefreshResult Fail()
        {
            return new RefreshResult
            {
                Success = false
            };
        }

        public static RefreshResult Ok(string accessToken, string refreshToken)
        {
            return new RefreshResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}

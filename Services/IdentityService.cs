using System.Security.Claims;
using TradePlatform.Api.Models;
using System.IdentityModel.Tokens.Jwt;


namespace TradePlatform.Api.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _http;

        public IdentityService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public (Guid userId, UserType userType) GetIdentity()
        {
            var user = _http.HttpContext?.User;

            if (user == null)
                throw new Exception("No HttpContext or User available");

            var userIdClaim =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new Exception("Invalid or missing userId in token");

            if (!Enum.TryParse<UserType>(roleClaim, true, out var userType))
                throw new Exception("Invalid or missing userType in token");

            return (userId, userType);
        }

        public Guid GetUserId() => GetIdentity().userId;
        public UserType GetUserType() => GetIdentity().userType;
        public string GetIpAddress()
        {
            var httpContext = _http.HttpContext;

            var ip = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
                ip = httpContext?.Connection.RemoteIpAddress?.ToString();

            return ip;
        }
        public string GetUserAgent()
        {
            return _http.HttpContext?.Request.Headers["User-Agent"].ToString();
        }
    }
}



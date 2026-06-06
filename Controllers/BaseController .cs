using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TradePlatform.Api.Models;
using static System.Net.WebRequestMethods;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly IHttpContextAccessor _http;

        public BaseController(IHttpContextAccessor http)
        {
            _http = http;
        }
            protected (Guid userId, UserType userType) GetIdentity()
            {
                var user = _http.HttpContext?.User;

                if (user == null)
                    throw new Exception("No HttpContext or User available");

                var userIdClaim =                    
                    user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value??
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    throw new Exception("Invalid or missing userId in token");

                if (!Enum.TryParse<UserType>(roleClaim, true, out var userType))
                    throw new Exception("Invalid or missing userType in token");

                return (userId, userType);
            }

            // 🔥 Convenience helpers
        protected Guid GetUserId() => GetIdentity().userId;
        protected UserType GetUserType() => GetIdentity().userType;
        protected Guid GetCurrentUserId()
        {
            var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id == null)
                throw new UnauthorizedAccessException("User id not found in token");

            return Guid.Parse(id);
        }
        protected IActionResult ApiOk(object? data = null)
        {
            return Ok(new { success = true, data });
        }

        protected IActionResult ApiError(object? data = null, int status = 400)
        {
            return StatusCode(status, new { success = false, data });
        }
        protected Guid ResolveEffectiveUser(Guid callerId, UserType callerType, Guid? targetUserId)
        {
            return callerType == UserType.admin && targetUserId.HasValue
                ? targetUserId.Value
                : callerId;
        }
        protected string GetIpAddress()
        {
            var httpContext = _http.HttpContext;

            var ip = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
                ip = httpContext?.Connection.RemoteIpAddress?.ToString();

            return ip;
        }
        protected string GetUserAgent()
        {
            return _http.HttpContext?.Request.Headers["User-Agent"].ToString();
        }

        protected string GenerateSlug(string name)
        {
            string slug = name.ToLowerInvariant();

            slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
            slug = slug.Trim('-');
            slug = Regex.Replace(slug, @"-+", "-");

            return slug;
        }

    }
}

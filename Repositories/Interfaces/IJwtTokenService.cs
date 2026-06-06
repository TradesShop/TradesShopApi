using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}

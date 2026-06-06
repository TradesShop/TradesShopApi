using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services
{
    public interface IIdentityService
    {
        Guid GetUserId();
        UserType GetUserType();
        (Guid userId, UserType userType) GetIdentity();

        string GetIpAddress();
        string GetUserAgent();
    }
}

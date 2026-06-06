using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> UserUpsertAsync(RegisterDto dto);
        Task<RegisterResponse> LoginAsync(LoginDto dto);
        Task<RefreshResult> RefreshTokensAsync(string refreshToken);
    }
}

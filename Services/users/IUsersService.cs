using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.users
{
    public interface IUsersService
    {
        Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<AccountContextDto> UserAccountGetAsync(Guid user_id);
        Task<User> GetUserByIdAsync(Guid user_id);
        Task<User> UpdateAnyUserAsync(UserDto uDto);
    }
}

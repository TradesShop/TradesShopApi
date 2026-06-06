using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPasswordHash, string newPasswordHash);
        Task<User> LoginAsync(string email,string passwordhash);
        Task<User> GetByEmailAsync(string email, int? usertype);
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetTradeUserByIdAsync(Guid userid);
        Task<User> UpdateAnyUserAsync(UserDto user);        
        Task UpdateStripeCustomerIdAsync(Guid userid, string stripeCustomerId);
        Task<AccountContextDto?> UserAccountGetAsync(Guid user_id);
        Task<User> GetUserByIdAsync(Guid user_id);
    }
}

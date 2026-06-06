using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.users
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepo;
        private readonly PasswordHashingService _passwordHashing;

        public UsersService(IUsersRepository usersRepo,
            PasswordHashingService passwordHashing)
        {
            _usersRepo = usersRepo;
            _passwordHashing = passwordHashing;
        }
        public async Task<CommonResponseDto> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            // Hash old password
            string oldHash = _passwordHashing.HashToBase64(oldPassword);
            // Hash new password
            string newHash = _passwordHashing.HashToBase64(newPassword);
            return await _usersRepo.ChangePasswordAsync(userId, oldHash, newHash);
        }
        public async Task<AccountContextDto> UserAccountGetAsync(Guid user_id)
        {
            return await _usersRepo.UserAccountGetAsync(user_id);
        }
        public async Task<User> GetUserByIdAsync(Guid user_id)
        {
            return await _usersRepo.GetUserByIdAsync(user_id);
        }
        public async Task<User> UpdateAnyUserAsync(UserDto uDto)
        {
            return await _usersRepo.UpdateAnyUserAsync(uDto);
        }
    }
}

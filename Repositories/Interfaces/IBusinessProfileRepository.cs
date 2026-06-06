using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IBusinessProfileRepository
    {
        
        Task<BusinessProfile?> GetByUserIdAsync(Guid user_id);
        Task<IntroMessageUpdateReqDto> business_intro_msg_update_async(IntroMessageUpdateReqDto introMsgDto);
    }
}

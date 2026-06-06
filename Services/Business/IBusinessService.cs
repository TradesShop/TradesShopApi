using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Business
{
    public interface IBusinessService
    {
        Task<List<BusinessCategorySkillResponseDto>> GetBusinessCategorySkillsAsync(Guid business_id);
        Task BusinessSkillsUpdateAsync(BusinessSkillsUpdateDto dto);
        Task<IEnumerable<UserAddress>> BusinessAddressesForUserAsync(Guid user_id);
        Task<BusinessProfileDto> BusinessProfileForUserAsync(Guid user_id);
        Task<BusinessProfileDto> BusinessProfileUpsertAsync(BusinessProfileDto bpDto);
        Task<IEnumerable<BusinessCategoryDto>> BusinessCategoryForUserAsync(Guid user_id);
        Task<UserAddress> BusinessAdressUpdateAsync(UserAddress model);
        Task<IEnumerable<BusinessWebProfileGetDto>> BusinessWebProfileForUserAsync(Guid business_id);
        Task BusinessWebProfileUpsert(BusinessWebProfileDto bwpDto);
    }
}

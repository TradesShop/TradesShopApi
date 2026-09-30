using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IBusinessRepository
{
	Task<List<BusinessCategorySkillFlatDto>> GetBusinessCategorySkillsAsync(Guid business_id);

	Task BusinessSkillsUpdateAsync(BusinessSkillsUpdateDto dto);

	Task<IEnumerable<BusinessCategoryDto>> BusinessCategoryForUserAsync(Guid user_id);

	Task<IEnumerable<UserAddress>> BusinessAddressesForUserAsync(Guid user_id);

	Task<UserAddress> BusinessPrimaryAddressForUserId(Guid user_id);

	Task<BusinessProfileDto> BusinessProfileForUserAsync(Guid user_id);

	Task<BusinessProfileDto> BusinessProfileUpsertAsync(BusinessProfileDto bpDto);

	Task<UserAddress> BusinessAdressUpdateAsync(UserAddress uaModel);

	Task<IEnumerable<BusinessWebProfileGetDto>> BusinessWebProfileForUserAsync(Guid business_id);

	Task<BusinessProfileDto> BusinessProfileForSlugAsync(string public_slug);

	Task BusinessWebProfileUpsert(BusinessWebProfileDto bwpDto);

	Task<int> BusinessMaxCategoriesForUserAsync(Guid user_id);

	Task<int> BusinessMaxLocationsForUserAsync(Guid user_id);

	Task<IEnumerable<UserAddress>> BusinessAdressRemoveAsync(AddressRemoveReq uaModel);
}

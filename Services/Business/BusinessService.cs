using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Business;

public class BusinessService : IBusinessService
{
	private readonly IBusinessRepository _businessRepository;

	public BusinessService(IBusinessRepository businessRepository)
	{
		_businessRepository = businessRepository;
	}

	public async Task<List<BusinessCategorySkillResponseDto>> GetBusinessCategorySkillsAsync(Guid business_id)
	{
		return (from x in await _businessRepository.GetBusinessCategorySkillsAsync(business_id)
			group x by new { x.business_id, x.category_id, x.category_name, x.is_primary } into g
			select new BusinessCategorySkillResponseDto
			{
				business_id = g.Key.business_id,
				category_id = g.Key.category_id,
				category_name = g.Key.category_name,
				is_primary = g.Key.is_primary,
				skills = (from x in g.Where((BusinessCategorySkillFlatDto x) =>
					{
						_ = x.category_skill_id;
						return true;
					})
					select new SkillDto
					{
						id = x.category_skill_id,
						name = x.category_skill_name
					}).ToList()
			} into x
			orderby x.is_primary descending, x.category_name
			select x).ToList();
	}

	public async Task<IEnumerable<UserAddress>> BusinessAddressesForUserAsync(Guid user_id)
	{
		return await _businessRepository.BusinessAddressesForUserAsync(user_id);
	}

	public async Task<IEnumerable<BusinessCategoryDto>> BusinessCategoryForUserAsync(Guid user_id)
	{
		return await _businessRepository.BusinessCategoryForUserAsync(user_id);
	}

	public async Task<BusinessProfileDto> BusinessProfileForUserAsync(Guid user_id)
	{
		return await _businessRepository.BusinessProfileForUserAsync(user_id);
	}

	public async Task<BusinessProfileDto> BusinessProfileUpsertAsync(BusinessProfileDto bpDto)
	{
		return await _businessRepository.BusinessProfileUpsertAsync(bpDto);
	}

	public async Task<UserAddress> BusinessAdressUpdateAsync(UserAddress model)
	{
		return await _businessRepository.BusinessAdressUpdateAsync(model);
	}

	public async Task BusinessSkillsUpdateAsync(BusinessSkillsUpdateDto dto)
	{
		await _businessRepository.BusinessSkillsUpdateAsync(dto);
	}

	public async Task<IEnumerable<BusinessWebProfileGetDto>> BusinessWebProfileForUserAsync(Guid business_id)
	{
		return await _businessRepository.BusinessWebProfileForUserAsync(business_id);
	}

	public async Task BusinessWebProfileUpsert(BusinessWebProfileDto bwpDto)
	{
		await _businessRepository.BusinessWebProfileUpsert(bwpDto);
	}

	public async Task<BusinessProfileDto> BusinessProfileForSlugAsync(string public_slug)
	{
		return await _businessRepository.BusinessProfileForSlugAsync(public_slug);
	}

	public async Task<int> BusinessMaxCategoriesForUserAsync(Guid user_id)
	{
		return await _businessRepository.BusinessMaxCategoriesForUserAsync(user_id);
	}

	public async Task<int> BusinessMaxLocationsForUserAsync(Guid user_id)
	{
		return await _businessRepository.BusinessMaxLocationsForUserAsync(user_id);
	}

	public async Task<IEnumerable<UserAddress>> BusinessAdressRemoveAsync(AddressRemoveReq uaModel)
	{
		return await _businessRepository.BusinessAdressRemoveAsync(uaModel);
	}
}

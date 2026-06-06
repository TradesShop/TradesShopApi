using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Data;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Credits;

namespace TradePlatform.Api.Services.Business
{
    public class BusinessService: IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;

        public BusinessService(
           IBusinessRepository businessRepository)
        {
            _businessRepository = businessRepository;
        }
        public async Task<List<BusinessCategorySkillResponseDto>> GetBusinessCategorySkillsAsync(Guid business_id)
        {
            var flat = await _businessRepository.GetBusinessCategorySkillsAsync(business_id);

            var result = flat
                .GroupBy(x => new { x.business_id, x.category_id })
                .Select(g => new BusinessCategorySkillResponseDto
                {
                    business_id = g.Key.business_id,
                    category_id = g.Key.category_id,
                    skills_ids = g.Select(x => x.category_skill_id).ToList()
                })
                .ToList();

            return result;
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
    }
}

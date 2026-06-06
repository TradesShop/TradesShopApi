using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class BusinessRepository: IBusinessRepository
    {
        private readonly DapperContext _context;
        private readonly IIdentityService _identity;

        public BusinessRepository(DapperContext context
            , IIdentityService identity)
        {
            _context = context;
            _identity = identity;
        }

        public async Task<IEnumerable<UserAddress>> BusinessAddressesForUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QueryAsync<UserAddress>(
                "usp_user_trade_addresses_get_async",
                new { user_id= user_id },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<List<BusinessCategorySkillFlatDto>> GetBusinessCategorySkillsAsync(Guid business_id)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QueryAsync<BusinessCategorySkillFlatDto>(
                "usp_user_business_category_skills_get",
                new { business_id = business_id },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
        public async Task BusinessSkillsUpdateAsync(BusinessSkillsUpdateDto dto)
        {
            using var conn = _context.CreateOpenConnection();

            // Convert skills list to table-valued parameter
            var tvp = new DataTable();
            tvp.Columns.Add("skill_id", typeof(int));

            foreach (var id in dto.skills_ids)
                tvp.Rows.Add(id);

            var parameters = new DynamicParameters();
            parameters.Add("@id",dto.id);
            parameters.Add("@user_id", _identity.GetUserId());
            parameters.Add("@business_id", dto.business_id);
            parameters.Add("@category_id", dto.category_id);
            parameters.Add("@skills_ids", tvp.AsTableValuedParameter("dbo.IntList"));

            await conn.ExecuteAsync(
                "usp_user_business_skills_update",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<BusinessCategoryDto>> BusinessCategoryForUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QueryAsync<BusinessCategoryDto>(
                "usp_user_business_category_get_all",
                new { user_id = user_id },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
        public async Task<BusinessProfileDto> BusinessProfileForUserAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QueryFirstOrDefaultAsync<BusinessProfileDto>(
                "usp_user_business_profile_get_async",
                new { user_id = user_id },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<BusinessProfileDto> BusinessProfileUpsertAsync(BusinessProfileDto bpDto)           
        {
            var parameters = new
            {
                id=bpDto.id,
                user_id= bpDto.user_id,
                name = bpDto.name,
                description = bpDto.description,
                active_since=bpDto.active_since,
                website_url = bpDto.website_url,
                business_type_id=bpDto.business_type_id,
                number_of_employees=bpDto.number_of_employees,
                registration_number = bpDto.registration_number,
                service_radius_km=bpDto.service_radius_km,
                public_slug=bpDto.public_slug

            };
            using var conn = _context.CreateOpenConnection();
            var anyprofile= await conn.QueryFirstOrDefaultAsync<BusinessProfileDto>(
                "usp_user_business_profile_upsert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return anyprofile;
        }
        public async Task<UserAddress> BusinessAdressUpdateAsync(UserAddress uaModel)
        {
            var parameters = new
            {
                user_id = uaModel.user_id,
                business_id = uaModel.business_id,
                address_id = uaModel.address_id,
                address_line1 = uaModel.address_line1,
                address_line2 = uaModel.address_line2,
                town = uaModel.town,
                county = uaModel.county,
                postcode = uaModel.postcode,
                country_id = uaModel.country_id,
                latitude = uaModel.latitude,
                longitude = uaModel.longitude,
                address_type_id = uaModel.address_type_id,
                service_radius_km=uaModel.service_radius_km,
                is_primary= uaModel.is_primary

            };
            using var conn = _context.CreateOpenConnection();
            var anyadress = await conn.QueryFirstOrDefaultAsync<UserAddress>(
                "usp_user_trade_address_upsert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return anyadress;
        }
        public async Task<IEnumerable<BusinessWebProfileGetDto>> BusinessWebProfileForUserAsync(Guid business_id)
        {

            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<BusinessWebProfileGetDto>(
                "usp_user_business_web_profile_get",
                new { business_id = business_id },
                commandType: CommandType.StoredProcedure
            );
            return result;
            
        }
        public async Task BusinessWebProfileUpsert(BusinessWebProfileDto bwpDto)
        {
            var platforms = new List<(string platform, string url)>
                {
                    ("twitter", bwpDto.twitter_url),
                    ("facebook", bwpDto.facebook_url)
                };

            foreach (var item in platforms)
            {
                if (!string.IsNullOrWhiteSpace(item.url))
                {
                    using var conn = _context.CreateOpenConnection();
                    await conn.ExecuteAsync(
                        "usp_user_business_web_profile_upsert",
                        new { business_id = bwpDto.business_id, platform = item.platform, url = item.url },
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
        }


    }
}

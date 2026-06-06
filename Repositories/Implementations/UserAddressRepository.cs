using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly DapperContext _context;

        public UserAddressRepository(DapperContext context)
        {
            _context = context;
        }

        // -------------------------
        // CREATE / UPSERT
        // -------------------------
        public async Task<Guid> CreateCustomerProfileAsync(RegisterDto reg_dto)
        {
            using var conn = _context.CreateOpenConnection();

            var customer_id = await conn.QueryFirstOrDefaultAsync<Guid>(
                "usp_user_customer_profile_create",
                new
                {
                    user_id = reg_dto.user_id,                   
                    address_line1 = reg_dto.address_line1,
                    address_line2 = reg_dto.address_line2,
                    town = reg_dto.town,
                    county = reg_dto.county,
                    postcode = reg_dto.postcode,
                    country_id = reg_dto.country_id,
                    longitude = reg_dto.longitude,
                    latitude = reg_dto.latitude                   
                },
                commandType: CommandType.StoredProcedure
            );

            return customer_id;
        }
        public async Task<Guid> CreateTradeUserBusinessAsync(RegisterDto reg_dto)
        {
            using var conn = _context.CreateOpenConnection();

            var business_id = await conn.QueryFirstOrDefaultAsync<Guid>(
                "usp_user_trade_business_create",
                new
                {
                    user_id = reg_dto.user_id,
                    business_name = reg_dto.business_name,
                    address_line1 = reg_dto.address_line1,
                    address_line2 = reg_dto.address_line2,
                    town = reg_dto.town,
                    county = reg_dto.county,
                    postcode = reg_dto.postcode,
                    country_id = reg_dto.country_id,
                    longitude = reg_dto.longitude,
                    latitude = reg_dto.latitude,
                    primarytrade = reg_dto.primarytrade,
                    secondarytrade = reg_dto.secondarytrade,
                    public_slug=reg_dto.public_slug
                },
                commandType: CommandType.StoredProcedure
            );

            return business_id;
        }





        // -------------------------
        // GET BY ENTITY
        // -------------------------
        public async Task<IEnumerable<UserAddress>> GetByEntityAsync(Guid entity_id)
        {
            using var conn = _context.CreateConnection();

            var result = await conn.QueryAsync<UserAddress>(
                "usp_user_trade_addresses_get_async",
                new { entity_id },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
}
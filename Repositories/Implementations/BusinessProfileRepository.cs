using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class BusinessProfileRepository : IBusinessProfileRepository
    {
        private readonly DapperContext _context;

        public BusinessProfileRepository(DapperContext context)
        {
            _context = context;
        }

        //public async Task<BusinessProfile> UpsertAsync(
        //    Guid? id,
        //    Guid user_id,
        //    string? name,
        //    string? description,
        //    int? active_since,
        //    string? website_url,
        //    int? business_type_id,
        //    int? number_of_employees,
        //    string? registration_number,
        //    int? service_radius_km,
        //    bool verified,
        //    Guid? updated_by
        //)
        //{
        //    var parameters = new
        //    {
        //        id,
        //        user_id,
        //        name = string.IsNullOrWhiteSpace(name) ? null : name,
        //        description = string.IsNullOrWhiteSpace(description) ? null : description,
        //        active_since,
        //        website_url = string.IsNullOrWhiteSpace(website_url) ? null : website_url,
        //        business_type_id,
        //        number_of_employees,
        //        registration_number = string.IsNullOrWhiteSpace(registration_number) ? null : registration_number,
        //        service_radius_km,
        //        verified,
        //        updated_by
        //    };
        //    using var conn = _context.CreateOpenConnection();
        //    return await conn.QueryFirstOrDefaultAsync<BusinessProfile>(
        //        "usp_business_profile_upsert",
        //        parameters,
        //        commandType: CommandType.StoredProcedure
        //    );
        //}

        public async Task<BusinessProfile?> GetByUserIdAsync(Guid userId)
        {
            using var conn = _context.CreateOpenConnection();
            const string sql = @"
                SELECT TOP 1 *
                FROM [dbo].[business_profile]
                WHERE user_id = @user_id
                ORDER BY created_at DESC";

            return await conn.QueryFirstOrDefaultAsync<BusinessProfile>(
                sql,
                new { user_id = userId }
            );
        }

        public async Task<IntroMessageUpdateReqDto> business_intro_msg_update_async(IntroMessageUpdateReqDto introMsgDto)
        {
            var parameters = new
            {
                user_id=introMsgDto.user_id,
                default_intro_message= introMsgDto.default_intro_message
            };
            using var conn = _context.CreateOpenConnection();
            var anyresult= await conn.QueryFirstOrDefaultAsync<IntroMessageUpdateReqDto>(
                "usp_business_default_intro_msg_update",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return anyresult;
        }

    }
}

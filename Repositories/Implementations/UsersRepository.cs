
using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DapperContext _context;
        public UsersRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<CommonResponseDto> ChangePasswordAsync(Guid userId,string oldPasswordHash,string newPasswordHash)
        {
            using var connection = _context.CreateOpenConnection();

            var result = await connection.QueryFirstAsync<CommonResponseDto>(
                "usp_user_change_password",
                new
                {
                    user_id = userId,
                    old_password_hash = oldPasswordHash,
                    new_password_hash = newPasswordHash
                },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<AccountContextDto?> UserAccountGetAsync(Guid user_id)
        {
            using var connection = _context.CreateOpenConnection();

            return await connection.QueryFirstOrDefaultAsync<AccountContextDto>(
                "usp_user_account_context_get_async",
                new { user_id = user_id},
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<User?> LoginAsync(string email, string passwordhash)
        {
            using var connection = _context.CreateOpenConnection();

            return await connection.QueryFirstOrDefaultAsync<User>(
                "usp_users_login",
                new { email = email, passwordhash = passwordhash },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<User> GetByEmailAsync(string email, int? usertype )
        {
            using var conn = _context.CreateOpenConnection();

            var user = await conn.QueryFirstOrDefaultAsync<User>(
                "usp_users_get_by_email",
                new { Email = email, UserType = usertype },
                commandType: CommandType.StoredProcedure
            );

            return user;
        }

        //public async Task<User> GetByEmailAsync(string email)
        //{
        //    const string sql = "SELECT * FROM users WHERE email = @Email";
        //    using var conn = _context.CreateOpenConnection();
        //    return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        //}

        public async Task<User> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM users WHERE id = @Id";
            using var conn = _context.CreateOpenConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User> UpdateAnyUserAsync(UserDto user)
        {
           
            using var conn = _context.CreateOpenConnection();
            var anyuser=await conn.QueryFirstOrDefaultAsync<User>(
                "[dbo].[usp_user_upsert]",
                new
                {
                    firstname=user.firstname,
                    lastname=user.lastname,
                    email = user.email,
                    password_hash = user.password_hash,
                    phone = user.phone,
                    user_type = (int)user.user_type                   
                },
                commandType: System.Data.CommandType.StoredProcedure
            );
            return anyuser;
        }
        public async Task<User> GetTradeUserByIdAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();
            var user = await conn.QueryFirstOrDefaultAsync<User>(
              "usp_users_get_by_id_async",
              new { user_id = user_id },
              commandType: CommandType.StoredProcedure
          );
           return user;
           
        }
        public async Task<string?> GetStripeCustomerIdAsync(Guid userid)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<string>(
                "dbo.GetStripeCustomerId",
                new { user_id = userid },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task UpdateStripeCustomerIdAsync(Guid userid, string stripeCustomerId)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "dbo.usp_user_stripe_customerid_update",
                new { user_id = userid, stripe_customer_id = stripeCustomerId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<User> GetUserByIdAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();
            var user = await conn.QueryFirstOrDefaultAsync<User>(
              "usp_users_get_by_id_async",
              new { user_id = user_id },
              commandType: CommandType.StoredProcedure
          );
            return user;

        }
    }
}

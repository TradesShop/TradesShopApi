using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly DapperContext _context;

        public EmailVerificationRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Save OTP code for the given email.
        /// SQL procedure hashes the code internally.
        /// </summary>
        public async Task SaveCodeAsync(string email, string code, DateTime expires_at)
        {
            using var conn = _context.CreateOpenConnection();
            await conn.ExecuteAsync(
                "usp_UserEmailVerification_SubmitCode",
                new { email = email, code = code, expires_at = expires_at },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<bool> HasRecentCodeAsync(string email)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.ExecuteScalarAsync<int>(
                "usp_UserEmailVerification_HasRecentCode",
                new { email },
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }
        /// <summary>
        /// Verify OTP code for the given email.
        /// Returns true if the code is valid and not expired.
        /// </summary>
        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QuerySingleOrDefaultAsync<int?>(
                "usp_UserEmailVerification_VerifyCode",
                new { email = email, code = code },
                commandType: CommandType.StoredProcedure
            );

            return result.HasValue && result.Value == 1;
        }

        /// <summary>
        /// Check if a user already exists by email.
        /// </summary>
        public async Task<bool> UserExistsAsync(string email)
        {
            using var conn = _context.CreateOpenConnection();

            var exists = await conn.QuerySingleOrDefaultAsync<bool>(
                "sp_Users_CheckByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );

            return exists;
        }
    }
}

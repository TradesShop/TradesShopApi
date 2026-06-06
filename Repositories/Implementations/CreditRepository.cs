using System.Data;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class CreditRepository : ICreditRepository
    {
        private readonly DapperContext _context;

        public CreditRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task GrantAsync(CreditGrantRequest request)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_credit_grant_create",
                new
                {
                    user_id = request.user_id,
                    source = request.source,
                    reference_id = request.reference_id,
                    total_credits = request.total_credits,
                    expires_at = request.expires_at,
                    reference_type = request.reference_type,
                    metadata = request.metadata
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ConsumeAsync(CreditConsumeRequest request)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_credit_consume_fifo",
                new
                {
                    user_id = request.user_id,
                    credits_to_use = request.credits_to_use,
                    reference_type = request.reference_type,
                    reference_id = request.reference_id,
                    metadata = request.metadata
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RefundAsync(CreditRefundRequest request)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_credit_refund",
                new
                {
                    user_id = request.user_id,
                    credits_to_refund = request.credits_to_refund,
                    reference_type = request.reference_type,
                    reference_id = request.reference_id,
                    expires_at = request.expires_at,
                    metadata = request.metadata
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> GetBalanceAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.ExecuteScalarAsync<int>(
                "usp_credit_get_balance",
                new { user_id },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}

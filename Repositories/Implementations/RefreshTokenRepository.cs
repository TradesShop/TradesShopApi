using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class RefreshTokenRepository:IRefreshTokenRepository
    {
        private readonly DapperContext _context;

        public RefreshTokenRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            using var conn = _context.CreateOpenConnection();
            return await conn.QueryFirstOrDefaultAsync<RefreshToken>(
                "usp_RefreshTokens_GetByToken",
                new { token },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddAsync(RefreshToken token)
        {
            using var conn = _context.CreateOpenConnection();
            await conn.ExecuteAsync(
                "usp_RefreshTokens_Add",
                new
                {
                    user_id = token.user_id,
                    token = token.token,
                    expires_at = token.expires_at,
                    isrevoked = token.isrevoked
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            using var conn = _context.CreateOpenConnection();
             await conn.ExecuteAsync(
                "sp_RefreshTokens_Update",
                new
                {
                    id = token.id,
                    isrevoked = token.isrevoked
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}

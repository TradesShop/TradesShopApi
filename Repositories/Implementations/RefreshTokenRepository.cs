using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly DapperContext _context;

	public RefreshTokenRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<RefreshToken> GetByTokenAsync(string token)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { token };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<RefreshToken>("usp_RefreshTokens_GetByToken", param, null, null, commandType);
	}

	public async Task AddAsync(RefreshToken token)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { token.user_id, token.token, token.expires_at, token.isrevoked, token.isused };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_RefreshTokens_Add", param, null, null, commandType);
	}

	public async Task UpdateAsync(RefreshToken token)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { token.id, token.isrevoked, token.isused };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_RefreshTokens_Update", param, null, null, commandType);
	}

	public async Task RevokeRefreshTokenByToken(RefreshToken anyRefreshToken)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { anyRefreshToken.token, anyRefreshToken.user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_RefreshTokens_RevokeByToken", param, null, null, commandType);
	}
}

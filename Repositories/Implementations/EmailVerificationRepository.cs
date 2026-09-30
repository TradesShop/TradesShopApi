using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class EmailVerificationRepository : IEmailVerificationRepository
{
	private readonly DapperContext _context;

	public EmailVerificationRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task SaveCodeAsync(string email, string code, DateTime expires_at)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { email, code, expires_at };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_UserEmailVerification_SubmitCode", param, null, null, commandType);
	}

	public async Task<bool> HasRecentCodeAsync(string email)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { email };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.ExecuteScalarAsync<int>("usp_UserEmailVerification_HasRecentCode", param, null, null, commandType) == 1;
	}

	public async Task<bool> VerifyCodeAsync(string email, string code)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { email, code };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QuerySingleOrDefaultAsync<int?>("usp_UserEmailVerification_VerifyCode", param, null, null, commandType) == 1;
	}

	public async Task<bool> UserExistsAsync(string email)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			Email = email
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QuerySingleOrDefaultAsync<bool>("sp_Users_CheckByEmail", param, null, null, commandType);
	}
}

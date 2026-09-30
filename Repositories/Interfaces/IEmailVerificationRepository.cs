using System;
using System.Threading.Tasks;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IEmailVerificationRepository
{
	Task SaveCodeAsync(string email, string code, DateTime expires_at);

	Task<bool> HasRecentCodeAsync(string email);

	Task<bool> VerifyCodeAsync(string email, string code);

	Task<bool> UserExistsAsync(string email);
}

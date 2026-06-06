namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IEmailVerificationRepository
    {
        /// <summary>
        /// Save OTP code for the given email.
        /// </summary>
        Task SaveCodeAsync(string email, string code, DateTime expires_at);
        Task<bool> HasRecentCodeAsync(string email);
        /// <summary>
        /// Verify OTP code for the given email.
        /// Returns true if valid and not expired.
        /// </summary>
        Task<bool> VerifyCodeAsync(string email, string code);

        /// <summary>
        /// Check if a user already exists by email.
        /// </summary>
        Task<bool> UserExistsAsync(string email);
    }
}

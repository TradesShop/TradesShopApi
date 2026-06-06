namespace TradePlatform.Api.Services
{
    public interface IUserCreditsService
    {
        Task AllocateInitialCreditsAsync(Guid user_id, Guid subscription_id, int credits);
        Task<bool> ConsumeCreditsAsync(Guid user_id, int amount, Guid source_id, string reason);
        Task AllocateRenewalCreditsAsync(Guid subscription_id, int credits);
        Task AdminAdjustCreditsAsync(Guid user_id, int amount, string reason);
    }
}

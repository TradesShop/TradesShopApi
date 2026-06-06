using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IStripeEventsRepository
    {
        Task<StripeEvents?> GetByStripeEventIdAsync(string stripe_eventid);
     
        //Task<bool>InsertStripeEventAsync(string eventId, string type, string payload);
        //Task MarkStripeEventProcessedAsync(string eventId);
        // Webhook-related
        Task InsertStripeEventAsync(StripeEvents entity);
        Task MarkStripeEventProcessedAsync(StripeEvents entity);
    }
}

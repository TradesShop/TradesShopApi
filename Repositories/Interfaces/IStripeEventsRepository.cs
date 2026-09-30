using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IStripeEventsRepository
{
	Task<StripeEvents?> GetByStripeEventIdAsync(string stripe_eventid);

	Task InsertStripeEventAsync(StripeEvents entity);

	Task MarkStripeEventProcessedAsync(StripeEvents entity);
}

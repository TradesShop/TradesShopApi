using System;
using System.Threading.Tasks;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IStripeCustomerService
{
	Task<string> CreateCustomerAsync(Guid userId, string email);

	Task<string> GetCustomerIdAsync(Guid userId);

	Task UpdateCustomerEmailAsync(string stripeCustomerId, string newEmail);
}

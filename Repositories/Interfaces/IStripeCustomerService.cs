using Stripe;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IStripeCustomerService
    {
        //Task<Customer> CreateCustomerAsync(CustomerCreateOptions options);
        Task<string> CreateCustomerAsync(Guid userId, string email);
        Task<string> GetCustomerIdAsync(Guid userId);
        Task UpdateCustomerEmailAsync(string stripeCustomerId, string newEmail);
        //Task<Customer> GetCustomerAsync(string customerId);
        //Task<string?> GetStripeCustomerIdAsync(Guid userid);
        //Task<Customer> UpdateCustomerAsync(string customerId, CustomerUpdateOptions options);
        //Task<string> ResolveStripeCustomerIdAsync(Guid user_id);
    }
}

  
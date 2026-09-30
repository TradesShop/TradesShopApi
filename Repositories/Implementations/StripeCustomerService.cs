using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class StripeCustomerService : IStripeCustomerService
{
	private readonly CustomerService _customerService;

	public StripeCustomerService(StripeClient client)
	{
		_customerService = new CustomerService(client);
	}

	public async Task<string> CreateCustomerAsync(Guid userId, string email)
	{
		return (await _customerService.CreateAsync(new CustomerCreateOptions
		{
			Email = email,
			Metadata = new Dictionary<string, string> { 
			{
				"user_id",
				userId.ToString()
			} }
		})).Id;
	}

	public async Task<string> GetCustomerIdAsync(Guid userId)
	{
		throw new NotImplementedException();
	}

	public async Task UpdateCustomerEmailAsync(string stripeCustomerId, string newEmail)
	{
		await _customerService.UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
		{
			Email = newEmail
		});
	}
}

using Stripe;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services;

public class BillingServices : IBillingServices
{
	private readonly IIdentityService _identityService;

	private readonly StripeClient _stripeClient;

	private readonly IRefundRepository _billingRepo;

	public BillingServices(IIdentityService identityService, IRefundRepository billingRepo, StripeClient stripeClient)
	{
		_billingRepo = billingRepo;
		_identityService = identityService;
		_stripeClient = stripeClient;
	}
}

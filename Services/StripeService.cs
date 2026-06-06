using Dapper;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services
{
    public class StripeService : IStripeService
    {
        private readonly DapperContext _context;
        private readonly string _webhookSecret;

        private readonly StripeClient _client;
        private readonly PaymentMethodService _stripepaymentserv;
        private readonly CustomerService _stripecustomers;
        private readonly SubscriptionService _subscriptions;
        private readonly IUsersRepository _usersRepo;
        private readonly SetupIntentService _setupIntents;
        private readonly IPaymentMethodRepository _paymethodRepo;


        public StripeService(
            IUsersRepository usersRepo,
            DapperContext context,
            IConfiguration config,            
            IPaymentMethodRepository paymethodRepo,
            StripeClient client
        )
        {
            _context = context;
            _usersRepo = usersRepo;
            _paymethodRepo = paymethodRepo;

            _webhookSecret = config["Stripe:WebhookSecret"];
            _client = client;

            _setupIntents = new SetupIntentService(_client);
            _stripepaymentserv = new PaymentMethodService(_client);
            _stripecustomers = new CustomerService(_client);
            _subscriptions = new SubscriptionService(_client);
        }

        public Event VerifyWebhook(string json, string? signature)
        {
            try
            {
                return EventUtility.ConstructEvent(json, signature, _webhookSecret);
            }
            catch (StripeException ex)
            {
                throw new Exception($"Invalid Stripe webhook signature: {ex.Message}");
            }
        }

        public async Task<string> CreateSetupIntentAsync(Guid userId)
        {
            var customerId = await ResolveStripeCustomerIdAsync(userId);

            if (string.IsNullOrWhiteSpace(customerId))
                throw new Exception($"Stripe customerId is null or empty for user {userId}");

            var intent = await _setupIntents.CreateAsync(new SetupIntentCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" }
            });

            if (string.IsNullOrWhiteSpace(intent.ClientSecret))
                throw new Exception("Stripe returned a setup intent without a client secret.");

            return intent.ClientSecret;
        }

        public async Task<PaymentMethod_db> AttachPaymentMethodToCustomerAsync(Guid user_id, string paymentmethod_id)
        {
            var stripecustomer_id = await ResolveStripeCustomerIdAsync(user_id);
            if (string.IsNullOrWhiteSpace(stripecustomer_id))
                throw new Exception($"Stripe customerId is null or empty for user {user_id}");

            var attached = await _stripepaymentserv.AttachAsync(
                paymentmethod_id,
                new PaymentMethodAttachOptions
                {
                    Customer = stripecustomer_id
                }
            );

            var card = attached.Card;

            var model = new PaymentMethod_db
            {
                user_id = user_id,
                stripe_payment_method_id = attached.Id,
                brand = card.Brand,
               //displaybrand = card.DisplayBrand,
                last4 = card.Last4,
                exp_month = (int)card.ExpMonth,
                exp_year = (int)card.ExpYear,
                name_on_card = attached.BillingDetails?.Name,
                is_default = true
            };

            var newId = await _paymethodRepo.AddPaymentMethodAsync(model);
            model.id = newId;
            return model;
        }

        public async Task SetDefaultPaymentMethodAsync(Guid userId, string stripe_payment_method_id)
        {
            var stripe_customer_id = await ResolveStripeCustomerIdAsync(userId);

            await _stripepaymentserv.AttachAsync(
                stripe_payment_method_id,
                new PaymentMethodAttachOptions
                {
                    Customer = stripe_customer_id
                }
            );

            await _stripecustomers.UpdateAsync(
                stripe_customer_id,
                new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = stripe_payment_method_id
                    }
                }
            );

            await _paymethodRepo.SetDefaultPaymentMethodAsync(userId, stripe_payment_method_id);
        }

        public async Task DetachPaymentMethodAsync(Guid userId, string stripe_payment_method_id)
        {
            await _stripepaymentserv.DetachAsync(
                stripe_payment_method_id,
                new PaymentMethodDetachOptions()
            );

            await _paymethodRepo.SoftDeletePaymentMethodAsync(userId, stripe_payment_method_id);
        }

        public async Task<Subscription> CreateOrUpdateSubscriptionAsync(Guid userId, string priceId, string paymentMethodId)
        {
            var customerId = await ResolveStripeCustomerIdAsync(userId);

            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = priceId }
                },
                DefaultPaymentMethod = paymentMethodId,
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            return await _subscriptions.CreateAsync(options);
        }

        public async Task CancelSubscriptionAsync(string stripe_subscription_id)
        {
            var service = new SubscriptionService();
            var options = new SubscriptionCancelOptions
            {
                InvoiceNow = false,
                Prorate = false
            };
            await service.CancelAsync(stripe_subscription_id, options);

           
        }

        public async Task<string> ResolveStripeCustomerIdAsync(Guid user_id)
        {
            var user = await _usersRepo.GetTradeUserByIdAsync(user_id);
            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrEmpty(user.stripe_customer_id))
                return user.stripe_customer_id;

            var customer = await _stripecustomers.CreateAsync(new CustomerCreateOptions
            {
                Email = user.email,
                Name = $"{user.firstname} {user.lastname}"
            });

            await _usersRepo.UpdateStripeCustomerIdAsync(user_id, customer.Id);

            return customer.Id;
        }

        public async Task UpdatePaymentMethodAsync(
            string payment_method_id,
            string? name_on_card,
            int expMonth,
            int expYear)
        {
            var options = new PaymentMethodUpdateOptions
            {
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Name = name_on_card
                },
                Card = new PaymentMethodCardOptions
                {
                    ExpMonth = expMonth,
                    ExpYear = expYear
                }
            };

            await _stripepaymentserv.UpdateAsync(payment_method_id, options);
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using System.Globalization;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Credits;
using TradePlatform.Api.Services.Subscriptions;

namespace TradePlatform.Api.Services
{
    public class BillingServices : IBillingServices
    {
        private readonly IPlansRepository _plansRepository;
        private readonly IPaymentMethodRepository _paymentMethodsRepository;
        private readonly ISubscriptionsRepository _subscriptionsRepository;
        private readonly IInvoicesTshRepository _invoicesRepository;
        private readonly IInvoiceItemsRepository _invoiceItemsRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly ISubscriptionHistoryRepository _subscriptionHistoryRepository;
        private readonly IStripeCustomerService _stripeCustomerService;
        private readonly IStripeService _stripeService;
        private readonly ICreditService _creditService;
        private readonly IIdentityService _identityService;
        private readonly StripeClient _stripeClient;

        public BillingServices(
            IPlansRepository plansRepository,
            IPaymentMethodRepository paymentMethodsRepository,
            ISubscriptionsRepository subscriptionsRepository,
            IInvoicesTshRepository invoicesRepository,
            IInvoiceItemsRepository invoiceItemsRepository,
            IPaymentsRepository paymentsRepository,
            ISubscriptionHistoryRepository subscriptionHistoryRepository,
            IStripeCustomerService stripeCustomerService,
            IStripeService stripeService,
            ICreditService creditService,
            IIdentityService identityService,
            StripeClient stripeClient)
        {
            _plansRepository = plansRepository;
            _paymentMethodsRepository = paymentMethodsRepository;
            _subscriptionsRepository = subscriptionsRepository;
            _invoicesRepository = invoicesRepository;
            _invoiceItemsRepository = invoiceItemsRepository;
            _paymentsRepository = paymentsRepository;
            _subscriptionHistoryRepository = subscriptionHistoryRepository;
            _stripeCustomerService = stripeCustomerService;
            _stripeService = stripeService;
            _creditService = creditService;
            _identityService = identityService;
            _stripeClient = stripeClient;
        }

        // ------------------------------------------------------------
        // 1. SELECT SUBSCRIPTION (SetupIntent if no card)
        // ------------------------------------------------------------
        public async Task<SubscriptionSelectResponse> SelectSubscriptionAsync(
        Guid user_id,
        Guid plan_id,
        Guid plan_price_id)
        {
            // ------------------------------------------------------------
            // 1. Validate plan
            // ------------------------------------------------------------
            var plan = await _plansRepository.GetPlanByIdAsync(plan_id);
            if (plan == null)
                throw new Exception("Invalid plan");

            // ------------------------------------------------------------
            // 2. Validate plan price
            // ------------------------------------------------------------
            var price = await _plansRepository.GetPlanPriceByIdAsync(plan_price_id);
            if (price == null || price.plan_id != plan_id)
                throw new Exception("Invalid plan price");

            // ------------------------------------------------------------
            // 3. Resolve Stripe customer
            // ------------------------------------------------------------
            var stripe_customer_id = await _stripeService.ResolveStripeCustomerIdAsync(user_id);

            // ------------------------------------------------------------
            // 4. Check if user has a valid default payment method
            // ------------------------------------------------------------
            var default_pm_db = await _paymentMethodsRepository.GetDefaultPaymentMethodAsync(user_id);
            PaymentMethod default_pm = null;

            if (default_pm_db != null)
            {
                try
                {
                    var pmService = new PaymentMethodService(_stripeClient);
                    default_pm = await pmService.GetAsync(default_pm_db.stripe_payment_method_id);

                    // Must belong to this customer
                    if (default_pm == null ||
                        default_pm.CustomerId != stripe_customer_id ||
                        default_pm.Type != "card")
                    {
                        default_pm = null;
                    }

                    // Optional expiry check
                    if (default_pm?.Card != null)
                    {
                        var now = DateTime.UtcNow;
                        if (default_pm.Card.ExpYear < now.Year ||
                           (default_pm.Card.ExpYear == now.Year &&
                            default_pm.Card.ExpMonth < now.Month))
                        {
                            default_pm = null;
                        }
                    }
                }
                catch
                {
                    default_pm = null;
                }
            }

            // ------------------------------------------------------------
            // CASE A: No valid payment method → collect card
            // ------------------------------------------------------------
            if (default_pm == null)
            {
                return new SubscriptionSelectResponse
                {
                    requires_payment_method = true,
                    client_secret = null,
                    ready_for_subscription = false,
                    subscription_id = null,
                    status = "requires_payment_method"
                };
            }

            // ------------------------------------------------------------
            // CASE B: Payment method exists → auto-create subscription
            // ------------------------------------------------------------
            var result = await CreateSubscriptionAsync(user_id, plan_id, plan_price_id);

            return new SubscriptionSelectResponse
            {
                requires_payment_method = false,
                client_secret = result.client_secret,
                ready_for_subscription = true,
                subscription_id = result.subscription_id,
                status = result.status
            };
        }

        public async Task<SubscriptionSelectResponse> CreateSubscriptionAsync(
            Guid user_id,
            Guid plan_id,
            Guid plan_price_id)
        {
            // ------------------------------------------------------------
            // 0. Validate plan
            // ------------------------------------------------------------
            var anyplan = await _plansRepository.GetPlanPriceByPriceId(plan_price_id);
            if (anyplan == null)
                throw new Exception("Invalid plan price");

            // ------------------------------------------------------------
            // 1. Check existing subscription
            // ------------------------------------------------------------
            var existing = await _subscriptionsRepository.GetActiveSubscriptionForUserAsync(user_id);

            if (existing != null)
            {
                if (existing.plan_price_id != plan_price_id)
                {
                    return await UpdateSubscriptionAsync(
                        user_id,
                        plan_price_id,
                        anyplan.stripe_price_id,
                        existing.stripe_subscription_id
                    );
                }

                return new SubscriptionSelectResponse
                {
                    status = existing.status,
                    subscription_id = existing.id,
                    client_secret = null
                };
            }

            // ------------------------------------------------------------
            // 2. Resolve Stripe customer + default payment method
            // ------------------------------------------------------------
            var stripe_customer_id = await _stripeService.ResolveStripeCustomerIdAsync(user_id);

            var default_pm = await _paymentMethodsRepository.GetDefaultPaymentMethodAsync(user_id);
            if (default_pm == null)
                throw new Exception("No default payment method");

            var subscriptionService = new SubscriptionService(_stripeClient);

            // ------------------------------------------------------------
            // 3. Metadata
            // ------------------------------------------------------------
            var metadata = new Dictionary<string, string>
            {
                { "user_id", user_id.ToString() },
                { "plan_id", plan_id.ToString() },
                { "plan_price_id", plan_price_id.ToString() },
                { "subscription_type", anyplan.plan_type },
                { "credits", anyplan.credits_per_period.ToString() },
                { "source_type", "subscription" },
                { "updated_by", _identityService.GetUserId().ToString() }
                
            };
            // ------------------------------------------------------------
            // 4. Create subscription (AUTOPAY MODE)
            // ------------------------------------------------------------
            var subscription = await subscriptionService.CreateAsync(
                new SubscriptionCreateOptions
                {
                    Customer = stripe_customer_id,

                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = anyplan.stripe_price_id
                        }
                    },

                    // ✅ uses saved default card
                    DefaultPaymentMethod = default_pm.stripe_payment_method_id,

                    // ✅ correct autopay behavior
                    PaymentBehavior = "allow_incomplete",

                    Metadata = metadata,

                    Expand = new List<string>
                    {
                        "latest_invoice.payment_intent"
                    }
                }
            );           

            var anySubscriptionInvoiceDB = new SubscriptionEventProcessDto
            {

                stripe_subscription_id = subscription.Id,
                user_id = user_id,
                plan_price_id = plan_price_id,
                status = subscription.Status,
                current_period_start = subscription.StartDate,
                current_period_end = subscription.EndedAt,
                cancel_at_period_end = subscription.CancelAtPeriodEnd,
                canceled_at = subscription.CanceledAt,
                //trial_start = sub.TrialStart?.TrialStart,
                trial_end = subscription.TrialEnd,
                //stripe_event_id = subscription.even.Id,               
                event_type = "subscription.created.local",
                metadata_json = JsonConvert.SerializeObject(metadata),
                actor = "user",
                source = "api"
            };
            await _subscriptionsRepository.SubscriptionEventProcessUpdateAsync(anySubscriptionInvoiceDB);

            var invoiceService = new Stripe.InvoiceService(_stripeClient);
            var invoice = await invoiceService.GetAsync(
                subscription.LatestInvoiceId,
                new InvoiceGetOptions
                {
                    Expand = new List<string> { "payment_intent" }
                }
            );
           // var paymentIntentId = invoice.Metadata != null
           // ? invoice.Metadata["payment_intent"] // only if you stored it manually
            //: null;
            //var pid=invoice["payment_intent"]?.ToString();

            return new SubscriptionSelectResponse
            {               
                   requires_payment_method = false,
                   client_secret = null,
                   ready_for_subscription = true                   
            };
           
        }

        // ------------------------------------------------------------
        // 3. UPDATE SUBSCRIPTION (plan change)
        // ------------------------------------------------------------     
        public async Task<SubscriptionSelectResponse> UpdateSubscriptionAsync(
                Guid effective_userid,
                Guid new_plan_price_id,
                string new_stripe_price_id,
                string stripe_subscription_id
            )
        {
            // 1. Load subscription from DB (OLD values)
            var anyplan = await _plansRepository.GetPlanPriceByPriceId(new_plan_price_id);
            var sub = await _subscriptionsRepository.GetByStripeIdAsync(stripe_subscription_id);
            if (sub == null)
                throw new InvalidOperationException("Subscription not found");

            // 2. Validate state transition
            var currentStatus = StripeStatusMapper.MapStripeStatus(sub.status);
            var sm = new SubscriptionStateMachine(currentStatus);

            if (!sm.TryTransitionTo(currentStatus, out var reason))
                throw new InvalidOperationException(reason);

            // 3. Load subscription from Stripe (NEW values will come after update)
            var service = new SubscriptionService(_stripeClient);
            var stripeSub = await service.GetAsync(stripe_subscription_id);

            if (stripeSub.Status == "canceled" || stripeSub.Status == "incomplete_expired" )
            {
                // You cannot update this subscription
                // You must create a new one
                return await CreateSubscriptionAsync(effective_userid, anyplan.plan_id, new_plan_price_id);
            }

            var stripeItem = stripeSub.Items.Data.FirstOrDefault();
            if (stripeItem == null)
                throw new InvalidOperationException("Stripe subscription has no items");

            // 4. Build FULL metadata for audit + webhook processing
            var metadata = new Dictionary<string, string>
            {
                // Who changed it
                { "user_id", effective_userid.ToString() },
                { "plan_id", anyplan.plan_id.ToString() },
                { "plan_price_id", new_plan_price_id.ToString() },
                { "subscription_type", anyplan.plan_type },
                { "source_type", "subscription" },
                { "credits", anyplan.credits_per_period.ToString() },
                { "action", "subscription_updated" },
                { "updated_by",_identityService.GetUserId().ToString()},

            };

            // 5. Update Stripe subscription item (NO proration)
            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = stripeItem.Id,
                        Price = new_stripe_price_id
                    }
                },
                ProrationBehavior = "none",
                Metadata = metadata
            };

            var updated = await service.UpdateAsync(stripe_subscription_id, options);

            
            // 7. Return response
            return new SubscriptionSelectResponse
            {
                requires_payment_method = false,
                client_secret = null,
                ready_for_subscription = true,
                subscription_id = sub.id,
                status = updated.Status
            };
        }

        public async Task CancelSubscriptionAsync(
                Guid effective_userid,
                Guid plan_price_id,
                string stripe_subscription_id
               )
        {
            // 1. Load subscription from DB
            var sub = await _subscriptionsRepository.GetByStripeIdAsync(stripe_subscription_id);
            if (sub == null)
                throw new InvalidOperationException("Subscription not found");

            // 2. Validate state transition
            var currentStatus = StripeStatusMapper.MapStripeStatus(sub.status);
            var sm = new SubscriptionStateMachine(currentStatus);

            if (!sm.TryTransitionTo(SubscriptionStatus.Canceled, out var reason))
                throw new InvalidOperationException(reason);

            // 3. Load plan info (needed for metadata)
            var plan = await _plansRepository.GetPlanPriceByPriceId(sub.plan_price_id);
            if (plan == null)
                throw new InvalidOperationException("Plan not found");

            // 4. Build unified metadata for subscription + invoice processing
            var metadata = new Dictionary<string, string>
                {
                    // Required identifiers
                    { "user_id", effective_userid.ToString() },
                    { "plan_id", plan.plan_id.ToString() },
                    { "plan_price_id", sub.plan_price_id.ToString() },

                    // Plan classification
                    { "subscription_type", plan.plan_type },
                    { "credits", plan.credits_per_period.ToString() },

                    // 🔥 Universal routing key for BOTH subscription + invoice events
                    { "source_type", "subscription" },
                    // Audit fields
                    { "action", "subscription_canceled" },
                    { "updated_by",_identityService.GetUserId().ToString()},
                };

            // 5. Cancel in Stripe (with metadata)
            var service = new SubscriptionService(_stripeClient);
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false, // immediate cancel
                Metadata = metadata
            };
            await service.UpdateAsync(stripe_subscription_id, options);

        }

        // =====================================================================
        // EXTRA HELPERS (you can call these from other services/controllers)
        // =====================================================================

        // Bundle purchase → grant credits
        public async Task GrantBundleCreditsAsync(Guid user_id, Guid bundle_id, Guid payment_intent_id, int credits)
        {
            var expiresAt = DateTime.UtcNow.AddMonths(24);

            await _creditService.GrantAsync(new CreditGrantRequest
            {
                user_id = user_id,
                source = "bundle",
                reference_id = bundle_id,
                total_credits = credits,
                expires_at = expiresAt,
                reference_type = "bundle",
                metadata = JsonConvert.SerializeObject(new
                {
                    payment_intent_id
                })
            });
        }

        // Job purchase → consume credits
        public async Task ConsumeJobCreditsAsync(Guid user_id, Guid job_id, int credits_cost)
        {
            await _creditService.ConsumeAsync(new CreditConsumeRequest
            {
                user_id = user_id,
                credits_to_use = credits_cost,
                reference_type = "job",
                reference_id = job_id,
                metadata = null
            });
        }

        // Dispute refund → refund credits
        public async Task RefundJobCreditsAsync(Guid user_id, Guid job_id, Guid dispute_id, int credits_to_refund)
        {
            var expiresAt = DateTime.UtcNow.AddMonths(6);

            await _creditService.RefundAsync(new CreditRefundRequest
            {
                user_id = user_id,
                credits_to_refund = credits_to_refund,
                reference_type = "dispute",
                reference_id = dispute_id,
                expires_at = expiresAt,
                metadata = JsonConvert.SerializeObject(new
                {
                    job_id
                })
            });
        }

        // Promotion → grant credits
        public async Task GrantPromotionCreditsAsync(Guid user_id, Guid promotion_id, int credits)
        {
            var expiresAt = DateTime.UtcNow.AddMonths(6);

            await _creditService.GrantAsync(new CreditGrantRequest
            {
                user_id = user_id,
                source = "promotion",
                reference_id = promotion_id,
                total_credits = credits,
                expires_at = expiresAt,
                reference_type = "promotion",
                metadata = null
            });
        }
    }
}

using Stripe;
using Stripe.Checkout;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Bundles
{
    public class BundlePurchaseService : IBundlePurchaseService
    {
        private readonly PaymentIntentService _paymentIntentService;
        private readonly IBundlesCreditRepository _bundlesRepo;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IBundlePricesRepository _pricesRepo;
        private readonly IBundleOrdersRepository _ordersRepo;
        private readonly IStripeService _stripeservice;
        private readonly StripeClient _stripe;
        private readonly ILogger<BundlePurchaseService> _logger;
        private readonly IIdentityService _identityService;
        public BundlePurchaseService(
            PaymentIntentService paymentIntentService,
            IBundlesCreditRepository bundlesRepo,
            IPaymentMethodRepository paymentMethodRepository,
             IBundlePricesRepository pricesRepo,
            IBundleOrdersRepository ordersRepo,
            IStripeService stripeservice,
             ILogger<BundlePurchaseService> logger,
             IIdentityService identityService,
            StripeClient stripe)
        {
            _paymentIntentService = paymentIntentService;
            _bundlesRepo = bundlesRepo;
            _paymentMethodRepository = paymentMethodRepository;
            _pricesRepo = pricesRepo;
            _ordersRepo = ordersRepo;
            _stripeservice= stripeservice;
            _logger = logger;
            _identityService = identityService;
            _stripe = stripe;
        }

        // ------------------------------------------------------------
        // 1. Create Stripe Checkout Session for bundle purchase
        // ------------------------------------------------------------
        public async Task<string> CreateCheckoutSessionAsync(
            Guid user_id,
            Guid bundle_id,
            Guid bundle_price_id,
            string successUrl,
            string cancelUrl)
        {
            var bundle = await _bundlesRepo.GetByIdAsync(bundle_id);
            if (bundle == null)
                throw new Exception("Bundle not found");

            var price = await _pricesRepo.GetPricesByIdAsync(bundle_price_id);
            if (price == null)
                throw new Exception("Active price not found");
            var stripe_customer_id = await _stripeservice.ResolveStripeCustomerIdAsync(user_id);

            var default_pm_db = await _paymentMethodRepository.GetDefaultPaymentMethodAsync(user_id);
            if (default_pm_db == null)
                throw new Exception("No default payment method found for user");
            // ------------------------------------------------------------
            // Create bundle order (pending)
            // ------------------------------------------------------------
            var order = new BundleOrders
            {               
                user_id = user_id,
                bundle_price_id = bundle_price_id,
                stripe_session_id = "",
                stripe_price_id = price.stripe_price_id,
                amount = price.price,
                currency = price.currency,
                status = "pending",
                created_at = DateTime.UtcNow
            };

            var anyorder=await _ordersRepo.CreateAsync(order);

            // 5. Create PaymentIntent and charge default card
            var amountInMinor = (long)(price.price * 100m); // assuming price is decimal in major units

            var intentOptions = new PaymentIntentCreateOptions
            {
                Amount = amountInMinor,
                Currency = price.currency,
                Customer = stripe_customer_id,
                PaymentMethod = default_pm_db.stripe_payment_method_id,
                OffSession = true,
                Confirm = true,
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", user_id.ToString() },
                    { "bundle_order_id", anyorder.id.ToString() },
                    { "plan_price_id", bundle_price_id.ToString() },                    
                    { "source_type", "credit_bundle" },
                    { "action", "creditbundle_created" },
                    { "updated_by", _identityService.GetUserId().ToString() },
                    { "reason", "User purchased credit bundle (direct charge)" }
                }
            };
            PaymentIntent intent;

            try
            {
                intent = await _paymentIntentService.CreateAsync(intentOptions);
            }
            catch (StripeException ex) when (ex.StripeError?.Code == "authentication_required")
            {
                // Card requires 3D Secure — you can fallback to Checkout or return a special response
                // For now, mark order as requires_action and let frontend handle it if you want.
                // Or just throw and show a friendly message.
                throw new Exception("Card requires authentication. Please update your payment method.", ex);
            }


            return successUrl;
        }
        public async Task OnBundleCheckoutCompletedAsync(BundleCheckoutCompletedDto dto)
        {
            try
            {
                await _ordersRepo.BundleCheckoutCompletedAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to complete bundle checkout for order {bundle_order_id}",
                    dto.bundle_order_id);

                throw;
            }
        }
        public async Task OnBundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto)
        {
            try
            {
                await _ordersRepo.BundleOrderMarkFailedAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to complete bundle checkout for order {bundle_order_id}",
                    dto.bundle_order_id);

                throw;
            }
        }
    }
    
}

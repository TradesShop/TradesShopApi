using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Stripe;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Payments;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Payments;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentsRepository _payservice;
        private readonly IStripeService _stripeservice;
        private readonly StripeClient _stripe;
        private readonly IPaymentMethodRepository _paymentMethods;
        private readonly IUsersRepository _users;
        private readonly IIdentityService _identity;
        private readonly IPaymentTshIntentService _paymentIntentService;

        public PaymentsController(IPaymentsRepository payservice,
           StripeClient stripe,
        IPaymentMethodRepository paymentMethods,
        IUsersRepository users,
        IStripeService stripeservice,
        IIdentityService identity ,
        IPaymentTshIntentService paymentIntentService,
        IHttpContextAccessor http
        ) : base(http)
        {
            _payservice = payservice;
            _stripe = stripe;
            _paymentMethods = paymentMethods;
            _users = users;
            _stripeservice = stripeservice;
            _identity = identity;
            _paymentIntentService = paymentIntentService;
        }

        // -----------------------------
        // Identity Helpers
        // -----------------------------
        private Guid ResolveEffectiveUser(Guid callerId, UserType callerType, Guid? targetUserId)
        {
            return callerType == UserType.admin && targetUserId.HasValue
                ? targetUserId.Value
                : callerId;
        }
        

        // -----------------------------
        // 1. Setup Intent
        // -----------------------------
        [HttpPost("setup-intent")]
        public async Task<IActionResult> CreateSetupIntent([FromBody] SetupIntentDto? request)
        {
            var (callerId, callerType) = _identity.GetIdentity();
           
            var clientSecret = await _payservice.CreateSetupIntentAsync(
                callerId,
                callerType,
                request?.target_user_id // null-safe
            );
            return ApiOk(clientSecret);

        }

        // ---------------------------------------------------------
        // 2. ATTACH PAYMENT METHOD
        // ---------------------------------------------------------
        [HttpPost("attach")]
        public async Task<IActionResult> AttachPaymentMethod([FromBody] AttachPaymentMethodDto attach_dto)
        {
            var (callerId, callerType) = _identity.GetIdentity();
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                attach_dto?.target_user_id
            );
            var result = await _payservice.AttachPaymentMethodAsync(effectiveUserId, callerType,attach_dto.payment_method_id);

            return Ok(result);
        }
        // ---------------------------------------------------------
        // 3. GET PAYMENT METHODS
        // ---------------------------------------------------------
        [HttpGet("methods")]
        public async Task<IActionResult> GetMethods([FromQuery] Guid? target_user_id)
        {
            var (callerId, callerType) = _identity.GetIdentity();

            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                target_user_id
            );
            if (effectiveUserId == null) return Unauthorized();

            var methods = await _paymentMethods.GetPaymentMethodsAsync(effectiveUserId);

            return ApiOk(methods);
        }
        // -----------------------------
        // 3. Get Default Card
        // -----------------------------
        [HttpGet("methods/default")]
        public async Task<IActionResult> GetDefaultPaymentMethod()
        {
            var (user_id, callerType) = _identity.GetIdentity();
           
            //Guid effectiveUserId = ResolveEffectiveUser(
            //    callerId,
            //    callerType,
            //    pmDto?.target_user_id
            //);
            //if (effectiveUserId == null) return Unauthorized();
            var anymethod= await _paymentMethods.GetDefaultPaymentMethodAsync(user_id);
            return ApiOk(anymethod);
        }

        // -----------------------------
        // 3. Set Default Card
        // -----------------------------
        [HttpPost("methods/default/{id}")]
        public async Task<IActionResult> SetDefaultCard(string id, [FromBody] SetDefaultPaymentMethodDto? pmDto)
        {
            var (callerId, callerType) = _identity.GetIdentity();
            var stripe_payment_method_id = id;
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                pmDto?.target_user_id
            );
            //if (effectiveUserId == null) return Unauthorized();
            await _stripeservice.SetDefaultPaymentMethodAsync(effectiveUserId, stripe_payment_method_id);

            return ApiOk();
        }

        // -----------------------------
        // 4. Detach Card
        // -----------------------------
        [HttpPut("methods/detach/{id}")]
        public async Task<IActionResult> DetachCard(string id, [FromBody] DetachPaymentMethodDto? pmDto)
        {
            var (callerId, callerType) = _identity.GetIdentity();
            var stripe_payment_method_id = id;

            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                pmDto?.target_user_id
            );

            //if (effectiveUserId == null) return Unauthorized();

            await _stripeservice.DetachPaymentMethodAsync(effectiveUserId, stripe_payment_method_id);

            return ApiOk();
        }

        // -----------------------------
        // 5. Create or Update Subscription
        // -----------------------------
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
        {
            var (callerId, callerType) = _identity.GetIdentity();
            var subscription = await _payservice.SubscribeAsync(
                callerId,
                callerType,
                request.priceid,
                request.paymentmethodid,
                request.targetuserid
            );

            return Ok(subscription);
        }

        // -----------------------------
        // 6. Cancel Subscription
        // -----------------------------
        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionDto request)
        {
            var (userId, userType) = _identity.GetIdentity();
            await _payservice.CancelSubscriptionAsync(
                userId,
                userType,
                request.stripe_subscription_id,
                request.targetuserid
            );

            return ApiOk();
        }
        [HttpPut("methods/update/{id}")]
        public async Task<IActionResult> UpdatePaymentMethod([FromRoute] string id,[FromBody] PaymentMethodUpdateDto dto)
        {
            var (callerId, callerType) = _identity.GetIdentity();

            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                dto.target_user_id
            );

            // STEP 1: Update Stripe FIRST
            await _stripeservice.UpdatePaymentMethodAsync(
                id,
                dto.name_on_card,
                dto.exp_month,
                dto.exp_year
            );
            // 2. Update local DB second
            await _paymentMethods.UpdatePaymentMethodAsync(
                id,
                dto.name_on_card,
                dto.exp_month,
                dto.exp_year,
                effectiveUserId
            );
            return ApiOk();
            
        }
        //[HttpPost("start")]
        //public async Task<IActionResult> StartPayment([FromBody] StartPaymentRequestDto dto)
        //{
        //    var result = await _paymentIntentService.StartPaymentAsync(dto);
        //    return Ok(result);
        //}
    }
}


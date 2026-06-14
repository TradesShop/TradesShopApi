using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stripe;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BillingController : BaseController
    {
        private readonly IBillingServices _billingService;
        private readonly IPaymentMethodRepository _paymentMethods;
        private readonly IPaymentsRepository _payservice;

        public BillingController(
        IBillingServices billingService,
        IPaymentMethodRepository paymentMethod,
        IPaymentsRepository payservice,
        IHttpContextAccessor http
        ) : base(http)
        {
            _billingService = billingService;
            _paymentMethods = paymentMethod;
            _payservice = payservice;
        }


        [HttpPost("subscriptions/select")]
        public async Task<IActionResult> SelectSubscription([FromBody] SubscriptionSelectRequest req)
        {
            var (callerId, callerType) = GetIdentity();
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                req?.target_user_id
            );            
            var result = await _billingService.SelectSubscriptionAsync(
                effectiveUserId,
                req.plan_id,
                req.plan_price_id
            );
            return Ok(result);
        }
        [HttpPost("subscriptions/create")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionCreateRequest req)
        {
            var (callerId, callerType) = GetIdentity();
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                req?.target_user_id
            );
         
            var result = await _billingService.CreateSubscriptionAsync(
                effectiveUserId,
                req.plan_id,
                req.plan_price_id
            );

            return ApiOk(result);
        }
        // -----------------------------
        // 1. Setup Intent
        // -----------------------------
        [HttpPost("setup-intent")]
        public async Task<IActionResult> CreateSetupIntent([FromBody] SetupIntentDto? request)
        {
            var (callerId, callerType) = GetIdentity();

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
            var (callerId, callerType) = GetIdentity();
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                attach_dto?.target_user_id
            );
            var result = await _payservice.AttachPaymentMethodAsync(effectiveUserId, callerType, attach_dto.payment_method_id);

            return ApiOk(result);
        }




        // ---------------------------------------------------------
        // 3. GET PAYMENT METHODS
        // ---------------------------------------------------------
        [HttpGet("methods")]
        public async Task<IActionResult> GetMethods([FromQuery] Guid? target_user_id)
        {
            var (callerId, callerType) = GetIdentity();

            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                target_user_id
            );
            if (effectiveUserId == null) return Unauthorized();

            var methods = await _paymentMethods.GetPaymentMethodsAsync(effectiveUserId);

            return ApiOk(methods);
        }

        
        //[HttpPost("subscriptions/update")]
        //public async Task<IActionResult> UpdateSubscription([FromBody] SubscriptionUpdateRequest req)
        //{
        //    var userId = GetUserId();

        //    var result = await _billingService.UpdateSubscriptionAsync(
        //        userId,
        //        req.subscription_id,
        //        req.new_plan_price_id
        //    );

        //    return Ok(result);
        //}

        //// -----------------------------
        //// 2. Attach Card
        //// -----------------------------
        //[HttpPost("attach-card")]
        //public async Task<IActionResult> AttachCard([FromBody] AttachPaymentMethodDto request)
        //{
        //    var (userId, userType) = GetIdentity();
        //    var result = await _billingService.AttachCardAsync(
        //        userId,
        //        userType,
        //        request.payment_method_id,
        //        request.target_user_id
        //    );

        //    return Ok(result);
        //}

        //// -----------------------------
        //// 3. Set Default Card
        //// -----------------------------
        //[HttpPost("set-primary-card")]
        //public async Task<IActionResult> SetDefaultCard([FromBody] SetDefaultPaymentMethodDto request)
        //{
        //    var (userId, userType) = GetIdentity();
        //    await _billingService.SetDefaultCardAsync(
        //        userId,
        //        userType,
        //        request.stripe_payment_method_id,
        //        request.target_user_id
        //    );

        //    return Ok(new { success = true });
        //}



        // -----------------------------
        //// 5. Create or Update Subscription
        //// -----------------------------
        //[HttpPost("subscribe")]
        //public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
        //{
        //    var (userId, userType) = GetIdentity();
        //    var subscription = await _billingService.SubscribeAsync(
        //        userId,
        //        userType,
        //        request.priceid,
        //        request.paymentmethodid,
        //        request.targetuserid
        //    );

        //    return Ok(subscription);
        //}

        //// -----------------------------
        //// 6. Cancel Subscription
        //// -----------------------------
        //[HttpPost("cancel-subscription")]
        //public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionDto request)
        //{
        //    var (userId, userType) = GetIdentity();
        //    await _billingService.CancelSubscriptionAsync(
        //        userId,
        //        userType,
        //        request.subscription_id,
        //        request.targetuserid
        //    );

        //    return Ok(new { success = true });
        //}
    }
}

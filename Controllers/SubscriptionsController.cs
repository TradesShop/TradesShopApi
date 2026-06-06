using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TradePlatform.Api.Services.Subscriptions;
using static System.Net.WebRequestMethods;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    public class SubscriptionsController : BaseController
    {
        private readonly IUserSubscriptionService _service;

        public SubscriptionsController(
            IUserSubscriptionService service,
            IHttpContextAccessor http
        ) : base(http)
        {
            _service = service;
        }

        private Guid UserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public class CreateSubscriptionRequest
        {
            public string price_id { get; set; } = null!;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var (callerId, callerType) = GetIdentity();
            var anysubs = await _service.GetActiveSubscriptionForUserAsync(callerId);
            return ApiOk(anysubs);
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest req)
        //{
        //    return Ok(await _service.CreateOrUpdateSubscriptionAsync(UserId, req.price_id, UserId));
        //}

        //[HttpPost("cancel")]
        //public async Task<IActionResult> Cancel()
        //{
        //    await _service.CancelSubscriptionAsync(UserId, UserId);
        //    return NoContent();
        //}
    }
}

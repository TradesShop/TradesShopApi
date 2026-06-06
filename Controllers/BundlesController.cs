using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Services.Bundles;
using static System.Net.WebRequestMethods;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BundlesController : BaseController
    {
        private readonly IBundlePurchaseService _purchaseService;
        private readonly IBundleAdminService _adminService;

        public BundlesController(
            IBundlePurchaseService purchaseService,
            IBundleAdminService adminService,
            IHttpContextAccessor http
        ) : base(http)
        {
            _purchaseService = purchaseService;
            _adminService = adminService;
        }

        // ------------------------------------------------------------
        // 1. List active bundles (user-facing)
        // ------------------------------------------------------------
        [HttpGet("credit")]
        public async Task<IActionResult> GetActiveCreditBundles()
        {
            var bundles = await _adminService.GetAllBundlesAsync();
            return ApiOk(bundles);
        }

        // ------------------------------------------------------------
        // 2. Create checkout session for bundle purchase
        // ------------------------------------------------------------
        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] BundleSelectDto req)
        {
            var (callerId, callerType) = GetIdentity();
            Guid effectiveUserId = ResolveEffectiveUser(
                callerId,
                callerType,
                req?.target_user_id
            );
            var successUrl = "http://localhost:3000/my-account/membership/credits/success";
            var cancelUrl = "https://yourapp.com/bundles/cancel";

            var url = await _purchaseService.CreateCheckoutSessionAsync(
                effectiveUserId,
                req.bundle_id,
                req.bundle_price_id,
                successUrl,
                cancelUrl
            );

            return ApiOk(new { url });
        }
    }
}

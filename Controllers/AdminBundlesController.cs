using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Services.Bundles;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminBundlesController : ControllerBase
    {
        private readonly IBundleAdminService _adminService;

        public AdminBundlesController(IBundleAdminService adminService)
        {
            _adminService = adminService;
        }

        // ------------------------------------------------------------
        // 1. List all active bundles
        // ------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetBundles()
        {
            var bundles = await _adminService.GetActiveBundlesAsync();
            return Ok(bundles);
        }

        // ------------------------------------------------------------
        // 2. Get bundle details
        // ------------------------------------------------------------
        [HttpGet("{bundle_id}")]
        public async Task<IActionResult> GetBundle(Guid bundle_id)
        {
            var bundle = await _adminService.GetBundleAsync(bundle_id);
            if (bundle == null)
                return NotFound();

            return Ok(bundle);
        }

        // ------------------------------------------------------------
        // 3. Get prices for a bundle
        // ------------------------------------------------------------
        //[HttpGet("{bundle_id}/prices")]
        //public async Task<IActionResult> GetBundlePrices(Guid bundle_id)
        //{
        //    var prices = await _adminService.GetBundlePricesAsync(bundle_id);
        //    return Ok(prices);
        //}
    }
}

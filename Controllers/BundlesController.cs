using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Bundles;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BundlesController : BaseController
{
	private readonly IBundlePurchaseService _purchaseService;

	private readonly IBundleAdminService _adminService;

	public BundlesController(IBundlePurchaseService purchaseService, IBundleAdminService adminService)
	{
		_purchaseService = purchaseService;
		_adminService = adminService;
	}

	[HttpGet("credits")]
	public async Task<IActionResult> GetActiveCreditBundles()
	{
		return ApiOk(await _adminService.GetAllBundlesAsync());
	}
	[HttpPost("checkout")]
	public async Task<IActionResult> CreateCheckoutSession([FromBody] BundleSelectDto req, CancellationToken cancellationToken)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(callerId, callerType, req?.target_user_id);
		return ApiOk(await _purchaseService.CreateCheckoutSessionAsync(effectiveUserId, req.bundle_id, req.bundle_price_id, cancellationToken));
	}
    

}

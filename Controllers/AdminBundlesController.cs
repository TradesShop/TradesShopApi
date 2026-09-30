using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Bundles;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminBundlesController : ControllerBase
{
	private readonly IBundleAdminService _adminService;

	public AdminBundlesController(IBundleAdminService adminService)
	{
		_adminService = adminService;
	}

	[HttpGet]
	public async Task<IActionResult> GetBundles()
	{
		return Ok(await _adminService.GetActiveBundlesAsync());
	}

	[HttpGet("{bundle_id}")]
	public async Task<IActionResult> GetBundle(Guid bundle_id)
	{
		CreditBundles bundle = await _adminService.GetBundleAsync(bundle_id);
		if (bundle == null)
		{
			return NotFound();
		}
		return Ok(bundle);
	}
}

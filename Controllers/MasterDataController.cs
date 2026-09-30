using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Business;
using TradePlatform.Api.Services.MasterData;

namespace TradePlatform.Api.Controllers;

[Route("api/master")]
[ApiController]
public class MasterDataController : BaseController
{
	private readonly IMasterDataService _service;

	private readonly IBusinessService _businessService;

	public MasterDataController(IMasterDataService service, IBusinessService businessService)
	{
		_service = service;
		_businessService = businessService;
	}

	[HttpGet("statuses")]
	public async Task<IActionResult> MasterDataStatusesAsync([FromQuery] string entity_type, bool? is_active = null)
	{
		return ApiOk(await _service.MasterDataStatusesAsync(entity_type, is_active));
	}

	[HttpGet("filters")]
	public async Task<IActionResult> GetFilterMetadata()
	{
		(Guid, UserType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.Item2;
		var categories2 = (await _businessService.BusinessCategoryForUserAsync(user_id)).Select((BusinessCategoryDto c) => new
		{
			id = c.category_id,
			name = c.category_name
		}).ToList();
		var locations = (await _businessService.BusinessAddressesForUserAsync(user_id)).Select((UserAddress c) => new
		{
			id = c.address_id,
			postcode = c.postcode
		}).ToList();
		return ApiOk(new
		{
			categories = categories2,
			locations = locations
		});
	}

	[HttpGet("dashboard-stats")]
	public async Task<IActionResult> GetDashboardStats()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		return ApiOk(await _service.GetTraderDashboardStatsAsync(user_id));
	}
}

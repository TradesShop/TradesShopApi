using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.DTOs.SupportTicket;
using TradePlatform.Api.DTOs.Unsubscribe;
using TradePlatform.Api.Services.Business;
using TradePlatform.Api.Services.Categories;
using TradePlatform.Api.Services.PublicApi;
using TradePlatform.Api.Services.Reviews;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicApiController : BaseController
{
	private readonly IBusinessService _businessService;

	private readonly IReviewsService _reviewsService;

	private readonly IUsersService _userService;

	private readonly ICategoryService _ctgryService;

	private readonly IPublicApiService _publicService;

	public PublicApiController(IBusinessService businessService, IReviewsService reviewsService, IUsersService userService, ICategoryService ctgryService, IPublicApiService publicService)
	{
		_businessService = businessService;
		_reviewsService = reviewsService;
		_userService = userService;
		_ctgryService = ctgryService;
		_publicService = publicService;
	}

    [HttpGet("categories/services/sitemap")]
    public async Task<IActionResult> GetAllServicesForSiteMapAsync()
    {
        return ApiOk(await _ctgryService.GetAllServicesForSiteMapAsync());
    }

    [HttpGet("categories")]
	public async Task<IActionResult> GetCategoriesAsync()
	{
		return ApiOk(await _ctgryService.GetCategoriesForPublic());
	}
    [HttpGet("categories/services")]
    public async Task<IActionResult> GetCategoriesWithServicesForPublic()
    {
        return ApiOk(await _ctgryService.GetCategoriesWithServicesForPublic());
    }
    [HttpGet("categories/service/{service_slug}")]
    public async Task<IActionResult> GetCategoryServiceWithSeoForPublic(string service_slug)
    {
        return ApiOk(await _ctgryService.GetCategoryServiceWithSeoForPublic(service_slug));
    }

    [HttpGet("categories/{category_slug}")]
    public async Task<IActionResult> GetCategorySeoBySlugAsync(string category_slug)
    {
        return ApiOk(await _ctgryService.GetCategorySeoBySlugAsync(category_slug));
    }

    /*Category/service locations*/
    [HttpGet("service/locations/count")]
    public async Task<IActionResult> GetServiceLocationsSiteMapCountAsync()
    {
        var locs_count = await _ctgryService.GetServiceLocationsSiteMapCountAsync();
        return ApiOk(locs_count);
    }

    [HttpGet("service/locations/sitemap")]
	public async Task<IActionResult> GetServiceLocationsSiteMapAsync(int offset = 0,int limit = 25)
    {
        var AllLocations = await _ctgryService.GetServiceLocationsSiteMapAsync(offset, limit);
        return ApiOk(AllLocations);
    }
    [HttpGet("service/locations/by-region")]
    //[ResponseCache(Duration = 86400)] // Cache response for 24 hours
    public async Task<IActionResult> GetLocationsGroupedByRegionAsync()
    {
        var groupedLocations = await _ctgryService.GetLocationsGroupedByRegionAsync();
        return Ok(groupedLocations);       
    }
	[HttpGet("service/location/{location_slug}")]
	public async Task<IActionResult> GetLocationSeoBySlugAsync(string location_slug)
	{
        var anyloc_seo = await _ctgryService.GetLocationSeoBySlugAsync(location_slug);
        return ApiOk(anyloc_seo);
	}


	//[HttpGet("categories/service/locations/{location_slug}")]
	//public async Task<IActionResult> GetServiceLocationWithSeoForPublic(string location_slug)
	//{
	//    return ApiOk(await _ctgryService.GetServiceLocationWithSeoForPublic(location_slug));
	//}








	[HttpGet("business/profile/{slug}")]
	public async Task<IActionResult> BusinessProfileForSlugAsync(string slug)
	{
		return ApiOk(await _businessService.BusinessProfileForSlugAsync(slug));
	}

	[HttpPost("reviews/trader")]
	public async Task<IActionResult> GetReviewedReviewsAsync([FromBody] TraderReviewsDto tr_dto)
	{
		return ApiOk(await _reviewsService.GetReviewedReviewsForTrader(tr_dto));
	}

	[HttpPost("unsubscribe/preferences/upsert")]
	public async Task<IActionResult> UserNotificationPreferencesUpsert([FromBody] UserNotificationPrefUpd unpUpd)
	{
		UserNotificationPreferences anyresult = await _userService.UserNotificationPreferencesUpsert(unpUpd);
		if (!anyresult.success)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpPost("unsubscribe/preferences/view")]
	public async Task<IActionResult> UserNotificationPreferencesForUser([FromBody] UserNotificationPrefReq unpReq)
	{
		UserNotificationPreferences anyresult = await _userService.UserNotificationPreferencesForUser(unpReq);
		if (!anyresult.success)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpPost("contact-us")]
	[AllowAnonymous]
	public async Task<IActionResult> CreateSupportTicket([FromBody] CreateSupportTicketDto tckdto)
	{
		tckdto.ip_address = GetIpAddress();
		return ApiOk(await _publicService.CreateSupportTicket(tckdto));
	}
}

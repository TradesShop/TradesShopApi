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

	[HttpGet("categories")]
	public async Task<IActionResult> GetCategoriesAsync()
	{
		return ApiOk(await _ctgryService.GetCategoriesForPublic());
	}

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

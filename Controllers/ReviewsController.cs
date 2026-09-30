using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Reviews;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : BaseController
{
	private readonly IReviewsRepository _reviewsRepo;

	private readonly IReviewsService _reviewsService;

	public ReviewsController(IReviewsRepository reviewsRepo, IReviewsService reviewsService)
	{
		_reviewsRepo = reviewsRepo;
		_reviewsService = reviewsService;
	}

	[HttpPost("awaiting")]
	public async Task<IActionResult> GetAwaitingReviewsAsync()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		return ApiOk(await _reviewsRepo.GetAwaitingReviewsAsync(callerId));
	}

	[HttpPost("request")]
	public async Task<IActionResult> ReviewRequestCreateAsync([FromBody] ReviewRequestDto rrDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		_ = identity.userId;
		_ = identity.userType;
		return ApiOk(await _reviewsRepo.ReviewRequestCreateAsync(rrDto));
	}

	[HttpPost("submit")]
	public async Task<IActionResult> SubmitReview([FromBody] ReviewSubmitDto rsDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		rsDto.homeowner_id = rsDto?.homeowner_id ?? callerId;
		review_request_meta result = await _reviewsService.SubmitReviewAsync(rsDto);
		if (result.success == false)
		{
			return ApiError(result);
		}
		return ApiOk(result);
	}

	[HttpPost("reply")]
	public async Task<IActionResult> SubmitReviewReply([FromBody] ReviewReplySubmit rrsDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		rrsDto.user_id = user_id;
		ReviewReplyResponse result = await _reviewsService.SubmitReviewReplyAsync(rrsDto);
		if (!result.success)
		{
			return ApiError(result);
		}
		return ApiOk(result);
	}

	[HttpPost("trader")]
	public async Task<IActionResult> GetReviewedReviewsAsync([FromBody] TraderReviewsDto tr_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		tr_dto.user_id = user_id;
		return ApiOk(await _reviewsRepo.GetReviewedReviewsForTrader(tr_dto));
	}
}

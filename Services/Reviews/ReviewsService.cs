using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Reviews;

public class ReviewsService : IReviewsService
{
	private readonly IReviewsRepository _reviewsRepo;

	private readonly IIdentityService _identityService;

	public ReviewsService(IReviewsRepository reviewsRepo, IIdentityService identityService)
	{
		_reviewsRepo = reviewsRepo;
		_identityService = identityService;
	}

	public async Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto)
	{
		if (!rsDto.job_purchase_id.HasValue && !rsDto.request_id.HasValue)
		{
			return new review_request_meta
			{
				success = false,
				message = "Either request_id or job_purchase_id must be provided."
			};
		}
		return await _reviewsRepo.SubmitReviewAsync(rsDto);
	}

	public async Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto)
	{
		return await _reviewsRepo.SubmitReviewReplyAsync(rrsDto);
	}

	public async Task<TraderReviewsResultDto?> GetReviewedReviewsForTrader(TraderReviewsDto tr_dto)
	{
		if (!tr_dto.user_id.HasValue && string.IsNullOrWhiteSpace(tr_dto.slug))
		{
			return new TraderReviewsResultDto
			{
				reviews = new List<ReviewsDto>(),
				total_records = 0
			};
		}
		return await _reviewsRepo.GetReviewedReviewsForTrader(tr_dto);
	}
}

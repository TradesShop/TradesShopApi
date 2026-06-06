using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Bundles;
using TradePlatform.Api.Services.Reviews;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : BaseController
    {
        private readonly IReviewsRepository _reviewsRepo;

        private readonly IReviewsService _reviewsService;

        public ReviewsController(
            IReviewsRepository reviewsRepo,
            IReviewsService reviewsService,
            IHttpContextAccessor http
        ) : base(http)
        {
            _reviewsRepo = reviewsRepo;
            _reviewsService = reviewsService;
        }
        [HttpPost("awaiting")]
        public async Task<IActionResult> GetAwaitingReviewsAsync()
        {
            var (callerId, callerType) = GetIdentity();
            
            var AwaitingReviews = await _reviewsRepo.GetAwaitingReviewsAsync(callerId);

            return ApiOk(AwaitingReviews);
        }
        [HttpPost("request")]
        public async Task<IActionResult> ReviewRequestCreateAsync([FromBody] ReviewRequestDto rrDto)
        {
            var (callerId, callerType) = GetIdentity();
            var anyrequest = await _reviewsRepo.ReviewRequestCreateAsync(rrDto);

            return ApiOk(anyrequest);
        }
        [HttpPost("reviewed")]
        public async Task<IActionResult> GetReviewedReviewsAsync()
        {
            var (user_id, callerType) = GetIdentity();
            var reviewed_list = await _reviewsRepo.GetReviewedReviewsForTrader(user_id);
            return ApiOk(reviewed_list);
        }
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitReview([FromBody] ReviewSubmitDto rsDto)
        {
            var (callerId, callerType) = GetIdentity();
            rsDto.homeowner_id = rsDto?.homeowner_id ?? callerId;
            var result = await _reviewsService.SubmitReviewAsync(rsDto);
            
            if (result.success==false)
                return ApiError(result);

            return ApiOk(result);
        }

        [HttpPost("reply")]
        public async Task<IActionResult> SubmitReviewReply([FromBody] ReviewReplySubmit rrsDto)
        {
            var (user_id, callerType) = GetIdentity();
            rrsDto.user_id = user_id;
            var result = await _reviewsService.SubmitReviewReplyAsync(rrsDto);
            if (result.success == false)
                return ApiError(result);

            return ApiOk(result);
        }
    }
}

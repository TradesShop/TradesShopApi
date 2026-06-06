
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Repositories.Interfaces;
namespace TradePlatform.Api.Services.Reviews
{
    public class ReviewsService:IReviewsService
    {
        private readonly IReviewsRepository _reviewsRepo;

        public ReviewsService(IReviewsRepository reviewsRepo)
        {
            _reviewsRepo = reviewsRepo;
        }

        public async Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto)
        {
            // Basic validation
            if (rsDto.job_purchase_id == null && rsDto.request_id == null)
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
    }
}

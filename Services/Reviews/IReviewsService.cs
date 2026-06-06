using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;

namespace TradePlatform.Api.Services.Reviews
{
    public interface IReviewsService
    {
        Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto);
        Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto);
    }
}

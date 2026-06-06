using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IReviewsRepository
    {
        Task<IEnumerable<AwaitingReviewDto>> GetAwaitingReviewsAsync(Guid user_id);
        Task<ReviewRequestResDto> ReviewRequestCreateAsync(ReviewRequestDto rrDto);
        Task<IEnumerable<ReviewsDto>> GetReviewedReviewsForTrader(Guid user_id);
        Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto);
        Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto);
    }
}

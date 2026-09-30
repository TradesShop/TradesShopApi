using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IReviewsRepository
{
	Task<IEnumerable<AwaitingReviewDto>> GetAwaitingReviewsAsync(Guid user_id);

	Task<ReviewRequestResDto> ReviewRequestCreateAsync(ReviewRequestDto rrDto);

	Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto);

	Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto);

	Task<TraderReviewsResultDto> GetReviewedReviewsForTrader(TraderReviewsDto tr_dto);

	Task<NotifyEmailDataModel> notify_email_review_requested(Guid review_request_id);

	Task<NotifyEmailDataModel> notify_email_review_posted(Guid review_id);

	Task<NotifyEmailDataModel> notify_email_review_replied(Guid review_id, int review_reply_id);
}

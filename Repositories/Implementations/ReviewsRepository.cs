using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class ReviewsRepository: IReviewsRepository
    {
        private readonly DapperContext _context;
        private readonly IIdentityService _identity;
        public ReviewsRepository(DapperContext context
            , IIdentityService identity
            )
        {
            _context = context;
            _identity = identity;
        }
        public async Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryFirstOrDefaultAsync<review_request_meta>(
                "usp_review_create",
                new
                {
                    job_purchase_id = rsDto.job_purchase_id,
                    request_id = rsDto.request_id,
                    job_id = rsDto.job_id,
                    homeowner_id = rsDto.homeowner_id,
                    overall_rating = rsDto.overall_rating,
                    title = rsDto.title,
                    comment = rsDto.comment,
                    would_recommend = rsDto.would_recommend,
                    ip_address = _identity.GetIpAddress(),
                },
                commandType: CommandType.StoredProcedure
            );

            return result ?? new review_request_meta
            {
                success = false,
                message = "Unknown error"
            };
        }
        public async Task<ReviewRequestResDto> ReviewRequestCreateAsync(ReviewRequestDto rrDto)
        {
            using var conn = _context.CreateOpenConnection();
            var anyrequest = await conn.QueryFirstOrDefaultAsync<ReviewRequestResDto>(
                    "usp_review_request_create",
                    new
                    {
                        job_id = rrDto.job_id,
                        job_purchase_id =rrDto.job_purchase_id
                    },
                    commandType: CommandType.StoredProcedure
            );
            return anyrequest;
        }

        public async Task<IEnumerable<AwaitingReviewDto>> GetAwaitingReviewsAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<AwaitingReviewDto>(
                "usp_review_request_awaiting_list",
                new { user_id = user_id },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        public async Task<IEnumerable<ReviewsDto>> GetReviewedReviewsForTrader(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<ReviewsDto>(
                "usp_reviews_get_for_trader",
                new { user_id = user_id },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
        public async Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryFirstOrDefaultAsync<ReviewReplyResponse>(
                "usp_review_reply_submit",
                new { 
                     review_id = rrsDto.review_id 
                    ,reply_text=rrsDto.reply_text
                    ,user_id= rrsDto.user_id
                },
                    commandType: CommandType.StoredProcedure
            );
            return result;
        }

    }
}

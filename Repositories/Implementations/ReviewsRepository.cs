using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Reviews;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.BackgroundEmailQueue;

namespace TradePlatform.Api.Repositories.Implementations;

public class ReviewsRepository : IReviewsRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	private readonly IBackgroundEmailQueue _backgroundEmailQueue;

	public ReviewsRepository(DapperContext context, IIdentityService identity, IBackgroundEmailQueue backgroundEmailQueue)
	{
		_context = context;
		_identity = identity;
		_backgroundEmailQueue = backgroundEmailQueue;
	}

	public async Task<review_request_meta> SubmitReviewAsync(ReviewSubmitDto rsDto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			job_purchase_id = rsDto.job_purchase_id,
			request_id = rsDto.request_id,
			job_id = rsDto.job_id,
			homeowner_id = rsDto.homeowner_id,
			overall_rating = rsDto.overall_rating,
			title = rsDto.title,
			comment = rsDto.comment,
			would_recommend = rsDto.would_recommend,
			ip_address = _identity.GetIpAddress()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		review_request_meta result = await conn.QueryFirstOrDefaultAsync<review_request_meta>("usp_review_create", param, null, null, commandType);
		Guid? review_id = result.review_id;
		if (review_id.HasValue)
		{
			Guid review_id2 = review_id.GetValueOrDefault();
			await _backgroundEmailQueue.QueueNotifyEmailForReviewPosted(review_id2);
		}
		return result ?? new review_request_meta
		{
			success = false,
			message = "Unknown error"
		};
	}

	public async Task<ReviewRequestResDto> ReviewRequestCreateAsync(ReviewRequestDto rrDto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { rrDto.job_id, rrDto.job_purchase_id };
		CommandType? commandType = CommandType.StoredProcedure;
		ReviewRequestResDto anyrequest = await conn.QueryFirstOrDefaultAsync<ReviewRequestResDto>("usp_review_request_create", param, null, null, commandType);
		Guid? review_request_id = anyrequest.review_request_id;
		if (review_request_id.HasValue)
		{
			Guid review_request_id2 = review_request_id.GetValueOrDefault();
			await _backgroundEmailQueue.QueueNotifyEmailForReviewRequested(review_request_id2);
		}
		return anyrequest;
	}

	public async Task<IEnumerable<AwaitingReviewDto>> GetAwaitingReviewsAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<AwaitingReviewDto>("usp_review_request_awaiting_list", param, null, null, commandType);
	}

	public async Task<TraderReviewsResultDto> GetReviewedReviewsForTrader(TraderReviewsDto tr_dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { tr_dto.user_id, tr_dto.slug, tr_dto.sort_by, tr_dto.page_number, tr_dto.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader result = await conn.QueryMultipleAsync("usp_reviews_get_for_trader", param, null, null, commandType);
		TraderReviewsResultDto anytr_result = new TraderReviewsResultDto();
		anytr_result.reviews = result.Read<ReviewsDto>().ToList();
		anytr_result.total_records = result.ReadSingle<int>();
		return anytr_result;
	}

	public async Task<ReviewReplyResponse> SubmitReviewReplyAsync(ReviewReplySubmit rrsDto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { rrsDto.review_id, rrsDto.reply_text, rrsDto.user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		ReviewReplyResponse result = await conn.QueryFirstOrDefaultAsync<ReviewReplyResponse>("usp_review_reply_submit", param, null, null, commandType);
		Guid reveiw_id = result.review_id;
		await _backgroundEmailQueue.QueueNotifyEmailForReviewReplied(reveiw_id, result.reply_id);
		return result;
	}

	public async Task<NotifyEmailDataModel> notify_email_review_requested(Guid review_request_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { review_request_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<NotifyEmailDataModel>("[dbo].[usp_notify_email_review_requested]", param, null, null, commandType);
	}

	public async Task<NotifyEmailDataModel> notify_email_review_posted(Guid review_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { review_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<NotifyEmailDataModel>("[dbo].[usp_notify_email_review_posted]", param, null, null, commandType);
	}

	public async Task<NotifyEmailDataModel> notify_email_review_replied(Guid review_id, int review_reply_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { review_id, review_reply_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<NotifyEmailDataModel>("[dbo].[usp_notify_email_review_replied]", param, null, null, commandType);
	}
}
